using Nativefy.DbContexts;
using Nativefy.Models.Media;

namespace Nativefy.Services.LocalMetadata;

public class LikedChannelsService : ILikedChannelsService
{
    public void AddToLikes(Channel channel)
    {
        var channels = NativefyDatabaseProvider.LikedChannels;
        if (channels == null)
            return;

        if (!channels.Exists(x => x.Id == channel.Id))
        {
            channels.Add(channel);
            NativefyDatabaseProvider.SaveLikedChannelsChanges();
        }
    }

    public bool TryGetChannel(string id, out Channel? channel)
    {
        channel = NativefyDatabaseProvider.LikedChannels.FirstOrDefault(c => c.Id == id);

        if (channel is null)
            return false;

        return true;
    }

    public List<Channel> GetChannels()
    {
        return NativefyDatabaseProvider.LikedChannels;
    }

    public bool TryRemoveFromLikes(string id, out Channel? channel)
    {
        channel = null;

        var channels = NativefyDatabaseProvider.LikedChannels;
        if (channels == null)
            return false;

        channel = channels.FirstOrDefault(c => c.Id == id);
        if (channel is null)
            return false;

        channels.Remove(channel);
        NativefyDatabaseProvider.SaveLikedChannelsChanges();
        return true;
    }

    public bool Exists(string id)
    {
        var channels = NativefyDatabaseProvider.LikedChannels;
        if (channels == null)
            return false;

        return channels.Exists(c => c.Id == id);
    }
}
