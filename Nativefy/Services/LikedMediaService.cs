using Microsoft.EntityFrameworkCore;
using Nativefy.DbContexts;
using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

public sealed class LikedMediaService(AppDbContext dbContext) : ILikedMediaService
{
    public async Task AddToLikesAsync(MediaItem item)
    {
        var playlist = await dbContext.LikedMedia.FirstOrDefaultAsync();
        if (playlist == null)
            return;

        if (!playlist.Items.Exists(x => x.Id == item.Id))
        {
            item.Position = playlist.Items.Count + 1;
            playlist.Items.Add(item);
            playlist.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<LikedMediaPlaylist?> GetLikedMediaAsync()
    {
        var playlist = await dbContext.LikedMedia.FirstOrDefaultAsync();
        return playlist;
    }

    public async Task RemoveFromLikesAsync(string id)
    {
        var playlist = await dbContext.LikedMedia.FirstOrDefaultAsync();

        if (playlist != null)
        {
            int removedCount = playlist.Items.RemoveAll(x => x.Id == id);
            if (removedCount > 0)
            {
                playlist.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
