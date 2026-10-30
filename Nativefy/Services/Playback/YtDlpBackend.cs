
using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace Nativefy.Services.Playback;

public sealed class YtDlpBackend(string executablePath = "C:\\Users\\Boba\\Downloads\\Videos\\yt-dlp.exe") : IStreamingBackend
{
    public async Task<MediaSource> CreateSourceAsync(
        MediaItem item,
        Resolution resolution,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        cancellationToken.ThrowIfCancellationRequested();

        int height = GetMaximumHeight(resolution);

        string videoUrl = $"https://www.youtube.com/watch?v={Uri.EscapeDataString(item.Id)}";

        string format = $"bestvideo[height<={height}][vcodec^=avc1]+bestaudio[acodec^=mp4a]";

        string[] urls = await ExtractUrlsAsync(videoUrl, format, cancellationToken);

        if (urls.Length != 2)
            throw new InvalidOperationException($"Expected 2 stream URLs, received {urls.Length}.");

        int port = GetAvailablePort();

        var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        string[] arguments =
        [
            "-hide_banner",
            "-loglevel", "warning",

            "-i", urls[0],
            "-i", urls[1],

            "-map", "0:v:0",
            "-map", "1:a:0",

            "-c:v", "copy",
            "-c:a", "copy",

            "-movflags", "+empty_moov+frag_keyframe+default_base_moof",
            "-frag_duration", "1000000",

            "-f", "mp4",
            "pipe:1"
        ];

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        var process = new Process
        {
            StartInfo = startInfo
        };

        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Could not start FFmpeg.");

            _ = ServeStreamAsync(listener, process);

            return MediaSource.CreateFromUri(new Uri($"http://127.0.0.1:{port}/stream.mp4"));
        }
        catch
        {
            listener.Close();

            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) { }

            process.Dispose();
            throw;
        }
    }

    private async Task<string[]> ExtractUrlsAsync(string videoUrl, string format, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        string[] arguments =
        [
            "--no-warnings",
            "--no-progress",
            "--no-playlist",
            "--socket-timeout", "15",
            "--format", format,
            "--get-url",
            videoUrl
        ];

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Could not start yt-dlp.");

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();

            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(cancellationToken);

            string output = await outputTask;
            string error = await errorTask;

            if (process.ExitCode != 0)
                throw new InvalidOperationException($"yt-dlp failed: {error}");

            string[] urls = output.Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

            foreach (string url in urls)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp &&
                     uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new InvalidOperationException("yt-dlp returned an invalid stream URL.");
                }
            }

            return urls;
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) { }

            throw;
        }
    }

    private static async Task ServeStreamAsync(HttpListener listener, Process process)
    {
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        try
        {
            while (true)
            {
                Task<HttpListenerContext> requestTask = listener.GetContextAsync();

                Task exitTask = process.WaitForExitAsync();

                Task completed = await Task.WhenAny(requestTask, exitTask);

                if (completed == exitTask)
                {
                    await exitTask;
                    break;
                }

                HttpListenerContext context = await requestTask;

                if (context.Request.HttpMethod == "HEAD")
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "video/mp4";
                    context.Response.Close();
                    continue;
                }

                if (context.Request.HttpMethod != "GET")
                {
                    context.Response.StatusCode = 405;
                    context.Response.Close();
                    continue;
                }

                HttpListenerResponse response = context.Response;

                response.StatusCode = 200;
                response.ContentType = "video/mp4";
                response.SendChunked = true;
                response.KeepAlive = false;

                try
                {
                    await process.StandardOutput.BaseStream.CopyToAsync(response.OutputStream);

                    response.OutputStream.Close();
                }
                finally
                {
                    response.Close();
                }

                break;
            }

            await process.WaitForExitAsync();

            string error = await errorTask;

            if (process.ExitCode != 0)
                Debug.WriteLine($"FFmpeg failed: {error}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Streaming error: {ex}");

            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) { }
        }
        finally
        {
            listener.Close();
            process.Dispose();
        }
    }

    private static int GetAvailablePort()
    {
        using var tcp = new TcpListener(IPAddress.Loopback, 0);

        tcp.Start();

        return ((IPEndPoint)tcp.LocalEndpoint).Port;
    }

    private static int GetMaximumHeight(Resolution resolution) =>
        resolution switch
        {
            Resolution.Standard360p => 360,
            _ => 360
        };
}