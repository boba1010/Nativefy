using Nativefy.Engine;
using Nativefy.Models.Media;
using Nativefy.Services.Cache;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Path = System.IO.Path;

namespace Nativefy.Services.Playback;

public sealed class StreamingBackend : IStreamingBackend
{
    private readonly MediaCache _cache;
    private readonly string _ytDlpPath;
    private readonly string _videosDirectory;

    public StreamingBackend(MediaCache cache, string ytDlpPath = "yt-dlp.exe")
    {
        _cache = cache;
        _ytDlpPath = ytDlpPath;

        _videosDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nativefy", "Videos");

        Directory.CreateDirectory(_videosDirectory);
    }

    public async Task<MediaSource> CreateSourceAsync(
        MediaItem item,
        Resolution resolution,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        cancellationToken.ThrowIfCancellationRequested();

        // Check cache first.
        string? videoPath = await _cache.TryGetPathAsync(item.Id, resolution, cancellationToken);

        if (videoPath is null)
        {
            videoPath = await DownloadAsync(item.Id, resolution, progress, cancellationToken);

            await _cache.RegisterAsync(item.Id, resolution, videoPath, CancellationToken.None);
        }

        StorageFile file = await StorageFile.GetFileFromPathAsync(videoPath);

        return MediaSource.CreateFromStorageFile(file);
    }

    private async Task<string> DownloadAsync(
        string videoId,
        Resolution resolution,
        IProgress<DownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        var (height, fps) = GetResolution(resolution);

        string key = GetKey(videoId, resolution);

        // Every download gets its own isolated directory.
        string stagingDirectory = Path.Combine(_videosDirectory, $".download-{Guid.NewGuid():N}");

        Directory.CreateDirectory(stagingDirectory);

        string outputTemplate = Path.Combine(stagingDirectory, "download.%(ext)s");

        string format = GetFormatSelector(height, fps);

        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        string[] arguments =
        [
            "--no-playlist",
            "--newline",
            "--no-colors",
            "--force-overwrites",

            "--format", format,
            "--remux-video", "mp4",

            "--output", outputTemplate,

            "--progress-template",
            "download:progress:%(progress.downloaded_bytes)s|%(progress.total_bytes)s|%(progress.total_bytes_estimate)s|%(progress.speed)s|%(progress.eta)s",

            "--print",
            "after_move:filepath:%(filepath)s",

            "--progress",

            $"https://www.youtube.com/watch?v={Uri.EscapeDataString(videoId)}"
        ];

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        var errors = new StringBuilder();
        string? finalPath = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!process.Start())
                throw new InvalidOperationException("Could not start yt-dlp.");

            Task stdoutTask = ReadOutputAsync(
                process.StandardOutput,
                line =>
                {
                    if (line.StartsWith("filepath:", StringComparison.Ordinal))
                        finalPath = line["filepath:".Length..].Trim();

                    ParseProgress(line, progress);
                });

            Task stderrTask = ReadOutputAsync(
                process.StandardError,
                line =>
                {
                    errors.AppendLine(line);
                    ParseProgress(line, progress);
                });

            try
            {
                await process.WaitForExitAsync(cancellationToken);

                await Task.WhenAll(stdoutTask, stderrTask);

                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"yt-dlp failed with exit code {process.ExitCode}:\n{errors}");

                if (string.IsNullOrWhiteSpace(finalPath))
                    throw new InvalidOperationException("yt-dlp did not report the downloaded file path.");

                finalPath = Path.GetFullPath(finalPath);

                // Never accept a path outside our staging directory.
                string relativePath = Path.GetRelativePath(stagingDirectory, finalPath);

                if (Path.IsPathRooted(relativePath) ||
                    relativePath == ".." ||
                    relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("yt-dlp returned an unexpected output path.");
                }

                if (!File.Exists(finalPath))
                    throw new FileNotFoundException("Downloaded media file was not found.", finalPath);

                string destination = Path.Combine(_videosDirectory, $"{key}.mp4");

                // Commit only the completed file.
                File.Move(finalPath, destination, overwrite: true);

                return destination;
            }
            catch
            {
                await StopProcessAsync(process);

                // Drain both redirected streams before disposing the process.
                try
                {
                    await Task.WhenAll(stdoutTask, stderrTask);
                }
                catch (IOException) { }

                throw;
            }
        }
        finally
        {
            TryDeleteDirectory(stagingDirectory);
        }
    }

    private static async Task StopProcessAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }

        try
        {
            await process.WaitForExitAsync();
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static async Task ReadOutputAsync(StreamReader reader, Action<string> onLine)
    {
        while (await reader.ReadLineAsync() is { } line)
            onLine(line);
    }

    private static void ParseProgress(string line, IProgress<DownloadProgress>? progress)
    {
        const string prefix = "progress:";

        int index = line.IndexOf(prefix, StringComparison.Ordinal);

        if (index < 0)
            return;

        string payload = line[(index + prefix.Length)..];

        string[] parts = payload.Split('|');

        if (parts.Length != 5)
            return;

        if (!long.TryParse(parts[0], out long downloaded))
            return;

        long? total = long.TryParse(parts[1], out long actualTotal)
            ? actualTotal : long.TryParse(parts[2], out long estimatedTotal) ? estimatedTotal : null;

        double? speed = double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedSpeed)
                ? parsedSpeed : null;

        TimeSpan? eta = double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds) 
            ? TimeSpan.FromSeconds(seconds) : null;

        double percentage = total is > 0 ? Math.Clamp(downloaded * 100.0 / total.Value, 0, 100) : 0;

        progress?.Report(new DownloadProgress(percentage, downloaded, total, speed, eta));
    }

    private static (int Height, int Fps) GetResolution(Resolution resolution)
    {
        return resolution switch
        {
            Resolution.Low144p => (144, 30),
            Resolution.Low240p => (240, 30),
            Resolution.Standard360p => (360, 30),
            Resolution.Standard480p => (480, 30),

            Resolution.Hd720p => (720, 30),
            Resolution.Hd720p60 => (720, 60),

            Resolution.FullHd1080p => (1080, 30),
            Resolution.FullHd1080p60 => (1080, 60),

            Resolution.Qhd1440p => (1440, 30),
            Resolution.Qhd1440p60 => (1440, 60),

            _ => throw new ArgumentOutOfRangeException(nameof(resolution))
        };
    }

    private static string GetFormatSelector(int height, int fps)
    {
        string preferred = fps >= 60 ? $"bv[height<={height}][fps>=50]+ba" : $"bv[height<={height}][fps<=30]+ba";

        string fallback = $"bv[height<={height}]+ba";
        string combined = $"b[height<={height}]";

        return $"{preferred}/{fallback}/{combined}";
    }

    private static string GetKey(string videoId, Resolution resolution)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{videoId}:{resolution}"));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}