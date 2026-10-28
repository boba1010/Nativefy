using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

public interface ILikedMediaService
{
    public Task<LikedMediaPlaylist?> GetLikedMediaAsync();
    public Task AddToLikesAsync(MediaItem item);
    public Task RemoveFromLikesAsync(string id);
}
