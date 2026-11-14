using Nativefy.Engine;
using Nativefy.Settings;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Nativefy.Services.Cache;

public sealed partial class MediaCache
{
    private readonly string _directory;
    private readonly string _indexPath;

    private readonly SemaphoreSlim _gate = new(1, 1);

    private Dictionary<string, CacheRecord> _entries = [];
    private bool _initialized;

    public MediaCache(string? directory = null)
    {
        _directory = Path.GetFullPath(directory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nativefy", "Videos"));

        _indexPath = Path.Combine(_directory, "cache-index.json");

        Directory.CreateDirectory(_directory);
    }

    public static long MaxSizeBytes => (long)Config.MediaCacheSize * 1024 * 1024;

    public async Task<string?> TryGetPathAsync(string videoId, Resolution resolution, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);

        string key = GetKey(videoId, resolution);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            if (!_entries.TryGetValue(key, out var entry))
                return null;

            if (!File.Exists(entry.FilePath))
            {
                _entries.Remove(key);
                await SaveIndexAsync(cancellationToken);
                return null;
            }

            entry.LastAccessUtc = DateTime.UtcNow;

            await SaveIndexAsync(cancellationToken);

            return entry.FilePath;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RegisterAsync(string videoId, Resolution resolution, string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string fullPath = Path.GetFullPath(filePath);

        if (!IsInsideDirectory(fullPath))
            throw new InvalidOperationException("Media files must be stored inside the Nativefy Videos directory.");

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Downloaded media file was not found.", fullPath);

        long size = new FileInfo(fullPath).Length;

        if (size <= 0)
            throw new InvalidDataException("Cannot register an empty media file.");

        if (size > MaxSizeBytes)
            throw new IOException("Media file exceeds the configured cache capacity.");

        string key = GetKey(videoId, resolution);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            _entries[key] = new CacheRecord
            {
                VideoId = videoId,
                Resolution = resolution,
                FilePath = fullPath,
                SizeBytes = size,
                LastAccessUtc = DateTime.UtcNow
            };

            if (Config.DeleteDuplicateVideosWithDifferentResolutions)
                RemoveOtherResolutions(videoId, resolution);

            await EvictAsync(cancellationToken);
            await SaveIndexAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveAsync(string videoId, Resolution resolution, CancellationToken cancellationToken = default)
    {
        string key = GetKey(videoId, resolution);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            if (_entries.Remove(key, out var entry))
                TryDelete(entry.FilePath);

            await SaveIndexAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task TrimAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            await EvictAsync(cancellationToken);
            await SaveIndexAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<long> GetTotalSizeAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            return _entries.Values.Sum(x => x.SizeBytes);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EvictAsync(CancellationToken cancellationToken)
    {
        long totalSize = _entries.Values.Sum(x => x.SizeBytes);

        foreach (var candidate in _entries.OrderBy(x => x.Value.LastAccessUtc).ToArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (totalSize <= MaxSizeBytes)
                break;

            if (!TryDelete(candidate.Value.FilePath))
                continue;

            totalSize -= candidate.Value.SizeBytes;
            _entries.Remove(candidate.Key);
        }

        if (totalSize > MaxSizeBytes)
            throw new IOException("Unable to satisfy the configured cache capacity.");
    }

    private void RemoveOtherResolutions(string videoId, Resolution resolution)
    {
        foreach (var entry in _entries
            .Where(x => x.Value.VideoId == videoId && x.Value.Resolution != resolution)
            .ToArray())
        {
            if (TryDelete(entry.Value.FilePath))
                _entries.Remove(entry.Key);
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
            return;

        if (File.Exists(_indexPath))
        {
            try
            {
                await using var stream = new FileStream(
                    _indexPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    FileOptions.Asynchronous);

                _entries =
                    await JsonSerializer.DeserializeAsync(stream, CacheJsonContext.Default.DictionaryStringCacheRecord, cancellationToken) 
                    ?? [];
            }
            catch (JsonException)
            {
                _entries = [];
            }
        }

        foreach (var entry in _entries.ToArray())
        {
            string path = entry.Value.FilePath;

            if (!IsInsideDirectory(path) || !File.Exists(path))
            {
                _entries.Remove(entry.Key);
                continue;
            }

            // Reconcile the index with the actual file.
            entry.Value.SizeBytes = new FileInfo(path).Length;
        }

        _initialized = true;

        await SaveIndexAsync(cancellationToken);
    }

    private async Task SaveIndexAsync(CancellationToken cancellationToken)
    {
        string temporary = _indexPath + ".tmp";

        await using (var stream = new FileStream(
            temporary,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous))
        {
            await JsonSerializer.SerializeAsync(stream, _entries, CacheJsonContext.Default.DictionaryStringCacheRecord, cancellationToken);
        }

        File.Move(temporary, _indexPath, overwrite: true);
    }

    private bool IsInsideDirectory(string path)
    {
        string fullPath = Path.GetFullPath(path);

        string prefix = _directory.EndsWith(Path.DirectorySeparatorChar) ? _directory : _directory + Path.DirectorySeparatorChar;

        return fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetKey(string videoId, Resolution resolution)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{videoId}:{resolution}"));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);

            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private sealed class CacheRecord
    {
        public string VideoId { get; set; } = "";
        public Resolution Resolution { get; set; }
        public string FilePath { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime LastAccessUtc { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Dictionary<string, CacheRecord>))]
    private sealed partial class CacheJsonContext : JsonSerializerContext
    {
    }
}