using YoutubeExplode;
using YoutubeExplode.Playlists;

namespace Nativefy.Engine;

public sealed class PlaylistEngine(YoutubeClient youtube)
{
    public async Task<Playlist> GetAsync(string id)
    {
        var ytPlaylist = await youtube.Playlists.GetAsync(new(id));

        var videos = new List<PlaylistVideo>();
        await foreach (var video in youtube.Playlists.GetVideosAsync(id))
            videos.Add(video);

        return new(ytPlaylist, videos);
    }
}
