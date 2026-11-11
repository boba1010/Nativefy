using Microsoft.Win32.SafeHandles;
using Nativefy.Engine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Nativefy.Services.Playback;

/// <summary>
/// Caching range proxy. LibVLC plays http://127.0.0.1:{port}/{session}/video (and /audio) like a normal seekable file.
/// Behind it, each stream is a sparse file split into 1 MiB blocks: Range requests are answered from disk, missing
/// blocks are fetched from googlevideo on demand and saved, and a background task fills in the rest.
/// An entry only counts once it is complete (a .done marker is written), then it plays with no network or backend.
/// Entries are evicted least-recently-used first once the total exceeds <see cref="MaxBytes"/>.
/// </summary>
public sealed partial class VideoCache
{
    private readonly string _root;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private readonly object _lock = new();

    private HttpListener? _listener;
    private int _port;
    private Session? _session; // only the current track has a session
    private int _sessionCount;

    /// <summary>Default 2 GB. Entries are counted at their full file size, so this is a conservative limit.</summary>
    public long MaxBytes { get; set; } = 2L * 1024 * 1024 * 1024;

    public VideoCache()
    {
        _root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nativefy", "VideoCache");

        Directory.CreateDirectory(_root);

        // Anything without a .done marker is left over from an interrupted download.
        foreach (var entry in Directory.EnumerateFiles(_root).GroupBy(f => Path.GetFileNameWithoutExtension(f)))
        {
            if (!entry.Any(f => f.EndsWith(".done")))
                foreach (string file in entry)
                    TryDelete(file);
        }

        // If your API's URLs need headers, set them here, e.g.:
        // _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 ...");
    }

    // ───────────────────────── Public API ─────────────────────────

    /// <summary>True when the track can start without asking the backend for URLs (fully cached, or already open).</summary>
    public bool TryGetLocal(string id, Resolution resolution, [NotNullWhen(true)] out StreamUrls? local)
    {
        string key = Key(id, resolution);
        string done = PathFor(key, "done");

        lock (_lock)
        {
            try
            {
                if (_session?.Key != key && File.Exists(done))
                    OpenSessionLocked(key, null);
            }
            catch (IOException)
            {
                RemoveLocked(key); // marker without valid data
            }

            Session? session = _session;

            if (session?.Key == key)
            {
                try { if (File.Exists(done)) File.SetLastWriteTimeUtc(done, DateTime.UtcNow); } catch (IOException) { }

                local = session.ProxyUrls(_port);
                return true;
            }
        }

        local = null;
        return false;
    }

    /// <summary>Opens (or reuses) the caching session for a track and returns the local URLs to play.</summary>
    public StreamUrls Open(string id, Resolution resolution, StreamUrls remote)
    {
        ArgumentNullException.ThrowIfNull(remote);

        string key = Key(id, resolution);

        lock (_lock)
        {
            if (_session?.Key != key)
                OpenSessionLocked(key, remote);

            return _session!.ProxyUrls(_port);
        }
    }

    /// <summary>Stops the current session. What was downloaded is deleted at the next start unless the entry completed.</summary>
    public void Close()
    {
        lock (_lock)
            CloseLocked();
    }

    /// <summary>Deletes one entry (e.g. when its data turns out to be bad).</summary>
    public void Remove(string id, Resolution resolution)
    {
        lock (_lock)
            RemoveLocked(Key(id, resolution));
    }

    // ───────────────────────── Sessions ─────────────────────────

    private void OpenSessionLocked(string key, StreamUrls? remote)
    {
        CloseLocked();
        EnsureServerLocked();
        Evict(key);

        bool done = File.Exists(PathFor(key, "done"));

        if (!done) // stale partial data from an earlier attempt
        {
            foreach (string file in Directory.GetFiles(_root, key + ".*"))
                TryDelete(file);
        }

        bool hasAudio = done ? File.Exists(PathFor(key, "audio")) : !string.IsNullOrEmpty(remote?.Audio);

        // A null URL means "fully cached": the stream is served from its file only.
        CachedStream Create(string extension, string? url) => new(PathFor(key, extension), done ? null : url, _http);

        var session = new Session(key, ++_sessionCount, Create("video", remote?.Video), hasAudio ? Create("audio", remote?.Audio) : null);

        void OnCompleted() => OnStreamCompleted(session);

        session.Video.Completed += OnCompleted;

        if (session.Audio is not null)
            session.Audio.Completed += OnCompleted;

        _session = session;

        if (!done)
        {
            session.Video.StartFiller(MaxBytes);
            session.Audio?.StartFiller(MaxBytes);
        }
    }

    private void CloseLocked()
    {
        _session?.Dispose();
        _session = null;
    }

    private void RemoveLocked(string key)
    {
        if (_session?.Key == key)
            CloseLocked();

        foreach (string file in Directory.GetFiles(_root, key + ".*"))
            TryDelete(file);
    }

    private void OnStreamCompleted(Session session)
    {
        lock (_lock)
        {
            if (!ReferenceEquals(_session, session) || !session.IsComplete)
                return;

            try { File.WriteAllBytes(PathFor(session.Key, "done"), []); } catch (IOException) { }

            Evict(session.Key);
        }
    }

    // Deletes least-recently-used entries until the cache fits in MaxBytes.
    private void Evict(string protectedKey)
    {
        var entries = new DirectoryInfo(_root).GetFiles()
            .GroupBy(f => Path.GetFileNameWithoutExtension(f.Name))
            .Select(g => (g.Key, Files: g.ToList(), Size: g.Sum(f => f.Length), Time: g.Max(f => f.LastWriteTimeUtc)))
            .ToList();

        long total = entries.Sum(e => e.Size);

        foreach (var entry in entries.Where(e => e.Key != protectedKey).OrderBy(e => e.Time))
        {
            if (total <= MaxBytes)
                break;

            foreach (FileInfo file in entry.Files)
                TryDelete(file.FullName);

            total -= entry.Size;
        }
    }

    // ───────────────────────── Local HTTP server ─────────────────────────

    private void EnsureServerLocked()
    {
        if (_listener is not null)
            return;

        using (var probe = new TcpListener(IPAddress.Loopback, 0)) // HttpListener can't bind port 0 itself
        {
            probe.Start();
            _port = ((IPEndPoint)probe.LocalEndpoint).Port;
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
        _listener.Start();

        _ = Task.Run(AcceptLoopAsync);
    }

    private async Task AcceptLoopAsync()
    {
        HttpListener listener = _listener!;

        while (listener.IsListening)
        {
            try
            {
                HttpListenerContext context = await listener.GetContextAsync();
                _ = Task.Run(() => HandleAsync(context));
            }
            catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException)
            {
                return;
            }
        }
    }

    private async Task HandleAsync(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;

        try
        {
            Session? session;

            lock (_lock)
                session = _session;

            string[] parts = context.Request.Url!.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            CachedStream? stream = null;

            if (session is not null && parts.Length == 2 && parts[0] == session.Id.ToString())
                stream = parts[1] switch { "video" => session.Video, "audio" => session.Audio, _ => null };

            if (stream is null)
            {
                response.StatusCode = 404;
                response.Close();
                return;
            }

            CancellationToken ct = stream.Lifetime;

            await stream.EnsureInitializedAsync(ct);

            string? header = context.Request.Headers["Range"];
            bool ranged = !string.IsNullOrEmpty(header);

            if (!TryGetRange(header, stream.Length, out long start, out long end))
            {
                response.StatusCode = 416;
                response.AddHeader("Content-Range", $"bytes */{stream.Length}");
                response.Close();
                return;
            }

            response.StatusCode = ranged ? 206 : 200;
            response.ContentType = stream.ContentType;
            response.AddHeader("Accept-Ranges", "bytes");
            response.ContentLength64 = end - start + 1;

            if (ranged)
                response.AddHeader("Content-Range", $"bytes {start}-{end}/{stream.Length}");

            if (context.Request.HttpMethod != "HEAD")
                await stream.CopyRangeAsync(start, end, response.OutputStream, ct);

            response.Close();
        }
        catch (Exception ex)
        {
            // The player aborts its connection on every seek; that's normal.
            if (ex is not (HttpListenerException or IOException or OperationCanceledException or ObjectDisposedException))
                Debug.WriteLine($"Video cache proxy error: {ex.Message}");

            try { response.Abort(); } catch { }
        }
    }

    // "bytes=a-b", "bytes=a-" or "bytes=-n" (the last n bytes). No header means the whole stream.
    private static bool TryGetRange(string? header, long length, out long start, out long end)
    {
        start = 0;
        end = length - 1;

        if (string.IsNullOrEmpty(header))
            return length > 0;

        string[] range = header.Replace("bytes=", "").Split(',')[0].Trim().Split('-');

        if (range.Length != 2)
            return false;

        bool hasFrom = long.TryParse(range[0], out long from);
        bool hasTo = long.TryParse(range[1], out long to);

        if (hasFrom)
        {
            start = from;

            if (hasTo)
                end = Math.Min(to, end);
        }
        else if (hasTo)
        {
            start = Math.Max(0, length - to);
        }
        else
        {
            return false;
        }

        return start <= end;
    }

    // ───────────────────────── Helpers ─────────────────────────

    private string PathFor(string key, string extension) => Path.Combine(_root, $"{key}.{extension}");

    private static string Key(string id, Resolution resolution) =>
        $"{string.Concat(id.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'))}_{resolution}";

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    private sealed partial class Session(string key, int id, CachedStream video, CachedStream? audio) : IDisposable
    {
        public string Key { get; } = key;
        public int Id { get; } = id;
        public CachedStream Video { get; } = video;
        public CachedStream? Audio { get; } = audio;

        public bool IsComplete => Video.IsComplete && (Audio?.IsComplete ?? true);

        public StreamUrls ProxyUrls(int port) => new(
            $"http://127.0.0.1:{port}/{Id}/video",
            Audio is null ? null : $"http://127.0.0.1:{port}/{Id}/audio");

        public void Dispose()
        {
            Video.Dispose();
            Audio?.Dispose();
        }
    }

    /// <summary>One stream (video or audio): a sparse file in 1 MiB blocks, filled from a remote URL, or a complete file.</summary>
    private sealed partial class CachedStream : IDisposable
    {
        private const int BlockSize = 1024 * 1024;
        private const uint FsctlSetSparse = 0x000900C4;

        private readonly string _path;
        private readonly string? _url;
        private readonly HttpClient _http;
        private readonly object _sync = new();
        private readonly CancellationTokenSource _lifetime = new();
        private readonly Dictionary<int, Task> _inflight = [];

        private SafeFileHandle? _handle;
        private bool[] _have = [];
        private int _haveCount;
        private int _playhead;
        private Task? _initTask;

        public long Length { get; private set; }
        public string ContentType { get; private set; } = "application/octet-stream";
        public CancellationToken Lifetime => _lifetime.Token;
        public event Action? Completed;

        private int BlockCount => (int)((Length + BlockSize - 1) / BlockSize);

        public bool IsComplete
        {
            get
            {
                lock (_sync)
                    return _have.Length > 0 && _haveCount == _have.Length;
            }
        }

        public CachedStream(string path, string? url, HttpClient http)
        {
            _path = path;
            _url = url;
            _http = http;

            if (url is null) // fully cached: nothing to download
            {
                Length = new FileInfo(path).Length;
                _have = new bool[BlockCount];
                Array.Fill(_have, true);
                _haveCount = _have.Length;
                _handle = OpenHandle(path);
                _initTask = Task.CompletedTask;
            }
        }

        private static SafeFileHandle OpenHandle(string path) =>
            File.OpenHandle(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.Asynchronous);

        // ───── Initialization: the first request tells us the total size ─────

        public Task EnsureInitializedAsync(CancellationToken ct)
        {
            Task task;

            lock (_sync)
                task = _initTask ??= Task.Run(InitializeAsync);

            return task.WaitAsync(ct);
        }

        private async Task InitializeAsync()
        {
            CancellationToken ct = _lifetime.Token;

            (byte[] data, long total, string? type) = await RequestAsync(0, BlockSize - 1, ct);

            if (data.Length != Math.Min(BlockSize, total))
                throw new IOException("Unexpected size for the first block.");

            // Sparse file: writing far into it doesn't allocate or zero-fill the gap.
            using (var file = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                DeviceIoControl(file.SafeFileHandle, FsctlSetSparse, 0, 0, 0, 0, out _, 0);
                file.SetLength(total);
            }

            _handle = OpenHandle(_path);
            await RandomAccess.WriteAsync(_handle, data, 0, ct);

            lock (_sync)
            {
                Length = total;
                ContentType = type ?? ContentType;
                _have = new bool[BlockCount];
            }

            MarkHave(0);
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeviceIoControl(
            SafeFileHandle hDevice, uint dwIoControlCode,
            nint lpInBuffer, uint nInBufferSize,
            nint lpOutBuffer, uint nOutBufferSize,
            out uint lpBytesReturned, nint lpOverlapped);

        // ───── Blocks ─────

        public async Task EnsureBlockAsync(int block, CancellationToken ct)
        {
            await EnsureInitializedAsync(ct);

            Task? fetch;

            lock (_sync)
            {
                if (_have[block])
                    return;

                if (!_inflight.TryGetValue(block, out fetch))
                    _inflight[block] = fetch = Task.Run(() => FetchBlockAsync(block));
            }

            // The fetch isn't tied to this request: if the player aborts, the block is still saved.
            await fetch.WaitAsync(ct);
        }

        private async Task FetchBlockAsync(int block)
        {
            try
            {
                CancellationToken ct = _lifetime.Token;

                long start = (long)block * BlockSize;
                int size = (int)Math.Min(BlockSize, Length - start);

                (byte[] data, long total, _) = await RequestAsync(start, start + size - 1, ct);

                if (total != Length || data.Length != size)
                    throw new IOException("Unexpected block from the server.");

                await RandomAccess.WriteAsync(_handle!, data, start, ct);

                MarkHave(block);
            }
            finally
            {
                lock (_sync)
                    _inflight.Remove(block);
            }
        }

        private void MarkHave(int block)
        {
            bool completed;

            lock (_sync)
            {
                if (_have[block])
                    return;

                _have[block] = true;
                completed = ++_haveCount == _have.Length;
            }

            if (completed)
                Completed?.Invoke();
        }

        private async Task<(byte[] Data, long Total, string? Type)> RequestAsync(long start, long end, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, _url);
            request.Headers.Range = new RangeHeaderValue(start, end);

            using HttpResponseMessage response =
                await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            response.EnsureSuccessStatusCode();

            if (response.StatusCode != HttpStatusCode.PartialContent)
                throw new IOException("The server ignored the Range header.");

            long total = response.Content.Headers.ContentRange?.Length
                ?? throw new IOException("The server did not report the stream length.");

            return (await response.Content.ReadAsByteArrayAsync(ct), total, response.Content.Headers.ContentType?.MediaType);
        }

        // ───── Serving a byte range to the player ─────

        public async Task CopyRangeAsync(long start, long end, Stream output, CancellationToken ct)
        {
            byte[] buffer = new byte[64 * 1024];

            for (long position = start; position <= end;)
            {
                int block = (int)(position / BlockSize);

                Volatile.Write(ref _playhead, block);

                await EnsureBlockAsync(block, ct);

                long stop = Math.Min(Math.Min((block + 1L) * BlockSize, Length) - 1, end);

                while (position <= stop)
                {
                    int want = (int)Math.Min(buffer.Length, stop - position + 1);
                    int read = await RandomAccess.ReadAsync(_handle!, buffer.AsMemory(0, want), position, ct);

                    if (read <= 0)
                        throw new IOException("The cache file ended early.");

                    await output.WriteAsync(buffer.AsMemory(0, read), ct);
                    position += read;
                }
            }
        }

        // ───── Background filler: downloads the missing blocks, starting at the playhead ─────

        public void StartFiller(long maxBytes)
        {
            _ = Task.Run(async () =>
            {
                CancellationToken ct = _lifetime.Token;

                try
                {
                    await EnsureInitializedAsync(ct);

                    if (Length > maxBytes)
                        return; // bigger than the whole cache: just proxy it, don't download ahead

                    for (int failures = 0; ;)
                    {
                        int block = NextMissingBlock();

                        if (block < 0)
                            return;

                        try
                        {
                            await EnsureBlockAsync(block, ct);
                            failures = 0;
                        }
                        catch (Exception) when (!ct.IsCancellationRequested && ++failures < 5)
                        {
                            await Task.Delay(2000, ct);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!ct.IsCancellationRequested)
                        Debug.WriteLine($"Cache filler stopped: {ex.Message}");
                }
            });
        }

        // First block that isn't cached, scanning forward from the playhead and wrapping around.
        private int NextMissingBlock()
        {
            lock (_sync)
            {
                for (int i = 0; i < _have.Length; i++)
                {
                    int block = (Math.Min(Volatile.Read(ref _playhead), _have.Length - 1) + i) % _have.Length;

                    if (!_have[block])
                        return block;
                }

                return -1;
            }
        }

        public void Dispose()
        {
            _lifetime.Cancel();

            Task[] pending;

            lock (_sync)
                pending = [.. _inflight.Values];

            // Close the file once in-flight writes have stopped.
            _ = Task.WhenAll(pending).ContinueWith(_ => _handle?.Dispose());
        }
    }
}