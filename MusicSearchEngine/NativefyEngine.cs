using YoutubeExplode;

namespace Nativefy.Engine;

public sealed class NativefyEngine : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly YoutubeClient _youtube;

    public SearchEngine Search { get; }
    public PlaylistEngine Playlist { get; }
    public StreamingEngine Streaming { get; }

    public NativefyEngine()
    {
        var handler = new HttpClientHandler { UseCookies = false };
        _httpClient = new HttpClient(handler);

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        );

        _httpClient.DefaultRequestHeaders.Add("X-YouTube-Client-Name", "1");
        _httpClient.DefaultRequestHeaders.Add("X-YouTube-Client-Version", "2.20240301.00.00");

        _youtube = new YoutubeClient(_httpClient);

        Search = new(_youtube);
        Playlist = new(_youtube);
        Streaming = new(_youtube);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
