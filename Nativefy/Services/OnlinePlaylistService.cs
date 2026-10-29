using Nativefy.Engine;
using System.Threading.Tasks;

namespace Nativefy.Services;

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
            Items = MapVideos(playlist)
        };
    }

    private static List<Models.Media.MediaItem> MapVideos(Playlist playlist)
    {
        var videos = new List<Models.Media.MediaItem>(playlist.Media.Count);

        for (int i = 1; i <= playlist.Media.Count; i++)
        {
            var v = playlist.Media[i - 1];

            videos.Add(new()
            {
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
