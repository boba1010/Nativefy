using Nativefy.Engine.Playable;
using TubeSharp;
using TubeSharp.Videos;

namespace Nativefy.Engine;

public sealed class PlaylistEngine(YoutubeClient youtube)
{
    public async Task<PlayablePlaylist> GetAsync(string id)
    {
        var ytPlaylist = await youtube.Playlists.GetPlaylistAsync(id);

        var videos = new List<YoutubeVideo>();
        await foreach (var video in youtube.Playlists.GetVideosRangedAsync(id).Take(30))
            videos.Add(video);

        return new(ytPlaylist, videos);
    }
}
