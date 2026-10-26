using Nativefy.Engine;
using System.Threading.Tasks;

namespace Nativefy.Services;

public interface ILikedMediaService
{
    public Task<List<PlayableItemDto>> GetLikedItemsAsync();
    public Task AddToLikesAsync(IPlayableItem item);
    public Task RemoveFromLikesAsync(string id);
}
