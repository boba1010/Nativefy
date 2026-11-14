using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

public interface ILikedMediaService
{
    public LikedMediaPlaylist? GetLikedMedia();
    public void AddToLikes(MediaItem item);
    public void RemoveFromLikes(string id);
}
