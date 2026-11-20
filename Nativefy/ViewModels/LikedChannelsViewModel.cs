using CommunityToolkit.Mvvm.ComponentModel;
using Nativefy.Models.Media;
using Nativefy.Services.LocalMetadata;
using System.Collections.ObjectModel;

namespace Nativefy.ViewModels;

public sealed partial class LikedChannelsViewModel(ILikedChannelsService likedChannels) : ObservableObject
{
    public ObservableCollection<Channel> Channels { get; } = [];

    public void LoadChannels()
    {
        Channels.Clear();

        var channels = likedChannels.GetChannels();
        foreach (var channel in channels)
            Channels.Add(channel);
    }

    public void UnlikeChannel(string id)
    {
        if (likedChannels.TryRemoveFromLikes(id, out _))
            LoadChannels();
    }
}
