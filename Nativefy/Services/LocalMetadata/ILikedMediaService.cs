using Nativefy.Models.Media;

namespace Nativefy.Services.LocalMetadata;

public interface ILikedMediaService
{
    public LikedMediaPlaylist? GetLikedMedia();
    public void AddToLikes(MediaItem item);
    public void RemoveFromLiked(string id);
}
