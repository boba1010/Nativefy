using Nativefy.Engine;
using Nativefy.Engine.Playable;
using System.Threading.Tasks;

namespace Nativefy.Services.OnlineMetadata;

public sealed class OnlinePlaylistService(NativefyEngine nativefy) : IOnlinePlaylistService
{
    public async Task<Models.Media.Playlist> GetPlaylistAsync(string id)
    {
        var playlist = await nativefy.Playlist.GetAsync(id);

        return new()
        {
            Id = playlist.Id,
            Name = playlist.Title,
            ImageUrl = playlist.ImageUrl,
            Url = playlist.Url,
            Author = playlist.Author,
            Items = MapVideos(playlist)
        };
    }

    private static List<Models.Media.MediaItem> MapVideos(PlayablePlaylist playlist)
    {
        var videos = new List<Models.Media.MediaItem>(playlist.Media.Count);

        for (int i = 1; i <= playlist.Media.Count; i++)
        {
            var v = playlist.Media[i - 1];

            videos.Add(new()
            {
                Id = v.Id,
                Url = v.Url,
                Author = v.Author,
                Duration = v.Duration,
                ImageUrl = v.ImageUrl,
                Title = v.Title,
                Position = i,
            });
        }

        return videos;
    }
}
