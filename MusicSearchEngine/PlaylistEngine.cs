using YoutubeExplode;
using YoutubeExplode.Playlists;

namespace Nativefy.Engine;

public sealed class PlaylistEngine : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly YoutubeClient _youtube;

    public PlaylistEngine()
    {
        var handler = new HttpClientHandler { UseCookies = false };
        _httpClient = new HttpClient(handler);

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        );

        _httpClient.DefaultRequestHeaders.Add("X-YouTube-Client-Name", "1");
        _httpClient.DefaultRequestHeaders.Add("X-YouTube-Client-Version", "2.20240301.00.00");

        _youtube = new YoutubeClient(_httpClient);
    }

    public async Task<Playlist> GetAsync(string id)
    {
        var ytPlaylist = await _youtube.Playlists.GetAsync(new(id));

        var videos = new List<PlaylistVideo>();
        await foreach (var video in _youtube.Playlists.GetVideosAsync(id))
            videos.Add(video);

        return new(ytPlaylist, videos);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
