using Microsoft.EntityFrameworkCore;
using Nativefy.DbContexts;
using Nativefy.Engine;
using System.Threading.Tasks;

namespace Nativefy.Services;

public sealed class LikedMediaService(AppDbContext dbContext) : ILikedMediaService
{
    private const string GlobalKey = "Global";

    public async Task AddToLikesAsync(IPlayableItem item)
    {
        var state = await dbContext.AppStates.FirstOrDefaultAsync(s => s.Id == GlobalKey);
        if (state == null)
        {
            state = new AppStateEntity { Id = GlobalKey };
            dbContext.AppStates.Add(state);
        }

        if (!state.LikedItems.Exists(x => x.Id == item.Id))
        {
            state.LikedItems.Add(new PlayableItemDto(item));
            state.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<PlayableItemDto>> GetLikedItemsAsync()
    {
        var state = await dbContext.AppStates.FirstOrDefaultAsync(s => s.Id == GlobalKey);
        return state?.LikedItems ?? [];
    }

    public async Task RemoveFromLikesAsync(string id)
    {
        var state = await dbContext.AppStates.FirstOrDefaultAsync(s => s.Id == GlobalKey);

        if (state != null)
        {
            int removedCount = state.LikedItems.RemoveAll(x => x.Id == id);
            if (removedCount > 0)
            {
                state.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
