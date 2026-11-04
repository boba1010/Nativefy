using Nativefy.DbContexts;
using Nativefy.Models.Media;

namespace Nativefy.Services;

public sealed class LikedMediaService : ILikedMediaService
{
    public void AddToLikes(MediaItem item)
    {
        var playlist = NativefyDatabaseProvider.LikedMedia;
        if (playlist == null)
            return;

        if (!playlist.Items.Exists(x => x.Id == item.Id))
        {
            item.Position = playlist.Items.Count + 1;
            playlist.Items.Add(item);
            playlist.UpdatedAt = DateTime.UtcNow;

            NativefyDatabaseProvider.SaveChanges();
        }
    }

    public LikedMediaPlaylist? GetLikedMedia()
    {
        var playlist = NativefyDatabaseProvider.LikedMedia;
        return playlist;
    }

    public void RemoveFromLikes(string id)
    {
        var playlist = NativefyDatabaseProvider.LikedMedia;

        if (playlist != null)
        {
            int removedCount = playlist.Items.RemoveAll(x => x.Id == id);
            if (removedCount > 0)
            {
                playlist.UpdatedAt = DateTime.UtcNow;
                NativefyDatabaseProvider.SaveChanges();
            }
        }
    }
}
