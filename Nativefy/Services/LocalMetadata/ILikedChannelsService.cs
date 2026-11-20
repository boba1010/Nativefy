using Nativefy.Models.Media;

namespace Nativefy.Services.LocalMetadata;

public interface ILikedChannelsService
{
    void AddToLikes(Channel channel);
    List<Channel> GetChannels();
    bool TryGetChannel(string id, out Channel? channel);
    bool TryRemoveFromLikes(string id, out Channel? channel);
    bool Exists(string id);
}
