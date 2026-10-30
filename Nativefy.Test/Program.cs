using System.Diagnostics;
using System.Net;

namespace Nativefy.Test;

internal class Program
{
    public static async Task Main()
    {
        using var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };

        using var client = new HttpClient(handler);

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/156.0.0.0 Safari/537.36");

        client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
        client.DefaultRequestHeaders.Referrer =
            new Uri("https://www.youtube.com/");

        client.DefaultRequestHeaders.ConnectionClose = false;

        using var response = await client.GetAsync(
            "https://rr2---sn-uxaxjvhxbt2u-j5pe7.googlevideo.com/videoplayback?expire=1793408511&ei=n-nkaqebEJbM1PIP-a3WoQ0&ip=197.57.208.149&id=o-AFNG4D1z_Q0Lx6mLbhmN6zqE9lB57fKTL9_giTWIQPjq&itag=134&source=youtube&requiressl=yes&xpc=EgVo2aDSNQ%3D%3D&cps=516&met=1793386911%2C&mh=yr&mm=18%2C29&mn=sn-uxaxjvhxbt2u-j5pe7%2Csn-hgn7yn7s&ms=aub%2Crdu&mv=m&mvi=2&pl=21&rms=aub%2Caub&initcwndbps=476750&bui=AZFAFb9MDHLuDiL2ahO2Fk1mZjfAemdVfdItujn349GREXYGnPHa71xmI0YjVyzcjmgNepOJWKYzYydp&spc=mpRQ_wUGgRtqLnocN5ZGqcGko1NNbEUhBLWvhCcPbMGq1TOPZZamHDm9J99O&vprv=1&svpuc=1&mime=video%2Fmp4&rqh=1&gir=yes&clen=1039385&dur=276.041&lmt=1768045241010649&mt=1793386635&fvip=4&epbp=1&keepalive=yes&fexp=51565115%2C52184237%2C52271470&c=VISIONOS&txp=5535534&sparams=expire%2Cei%2Cip%2Cid%2Citag%2Csource%2Crequiressl%2Cxpc%2Cbui%2Cspc%2Cvprv%2Csvpuc%2Cmime%2Crqh%2Cgir%2Cclen%2Cdur%2Clmt&sig=AE0s2JYwRgIhAJCkHs87R_RwfKfL51TkaVZfmRzkAAwULrCgty3SizoCAiEA8_obPZi0ciq49-uoBmUZ6eLVhSVfwKI8vDKG5bxe6X4%3D&lsparams=cps%2Cmet%2Cmh%2Cmm%2Cmn%2Cms%2Cmv%2Cmvi%2Cpl%2Crms%2Cinitcwndbps&lsig=APaTxxMwRQIhAJG9VPhluW0PREYGdKmnYBHXbpXQ2PMvGGeWGbAxjoSRAiB5nhbXmAu4lFfpNXzXG7tmL2RYdg4ihcZAhHGT_N8R_A%3D%3D",
            HttpCompletionOption.ResponseHeadersRead);

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
        Console.WriteLine($"Content-Length: {response.Content.Headers.ContentLength}");

        response.EnsureSuccessStatusCode();

        await using var input = await response.Content.ReadAsStreamAsync();
        await using var output = File.Create("video.mp4");

        var stopwatch = Stopwatch.StartNew();

        byte[] buffer = new byte[81920];
        long total = 0;

        while (true)
        {
            int read = await input.ReadAsync(buffer);

            if (read == 0)
                break;

            await output.WriteAsync(buffer.AsMemory(0, read));

            total += read;

            double speed = total / stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"{total:N0} bytes | {speed / 1024:F2} KB/s");
        }

        Console.WriteLine("DOWNLOAD COMPLETE");
    }
}