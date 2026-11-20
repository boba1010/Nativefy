using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Engine;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services.LocalMetadata;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class ChannelViewModel(NativefyEngine nativefy, ILikedChannelsService likedChannels) : BaseViewModel
{
    public string Id { get; set; } = null!;

    public string Url { get; set; } = null!;

    [ObservableProperty]
    public partial string Title { get; set; } = null!;

    [ObservableProperty]
    public partial string ImageUrl { get; set; } = "https://picsum.photos/400/400";

    public ObservableCollection<MediaItem> Videos { get; } = [];

    public ObservableCollection<byte> DummyVideos { get; } = [.. Enumerable.Repeat((byte)0, 100)];

    [ObservableProperty]
    public partial bool IsChannelLiked { get; set; }

    public async Task LoadChannelAsync(string id)
    {
        IsLoading = true;

        try
        {
            var channel = await nativefy.Channel.GetChannelAsync(id);
            Id = channel.Id;
            Title = channel.Title;
            ImageUrl = channel.ImageUrl;
            Url = channel.Url;
            foreach (var video in channel.Videos)
                Videos.Add(new MediaItem(video));

            IsChannelLiked = likedChannels.Exists(id);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadMoreAsync()
    {
    }

    public void Play(string id)
    {
        var video = Videos.FirstOrDefault(v => v.Id == id);
        if (video is null)
            return;

        WeakReferenceMessenger.Default.Send(new PlayItemMessage(new()
        {
            Id = video.Id,
            Url = video.Url,
            Author = video.Author,
            Duration = video.Duration,
            ImageUrl = video.ImageUrl,
            Title = video.Title,
            Position = 0,
        }));
    }

    [RelayCommand]
    public void ToggleLikeChannel()
    {
        if (likedChannels.Exists(Id))
        {
            if (likedChannels.TryRemoveFromLikes(Id, out _))
                IsChannelLiked = false;
            return;
        }

        if (!IsChannelLiked)
        {
            likedChannels.AddToLikes(new(Id, Title, ImageUrl, Url));
            IsChannelLiked = true;
        }
    }
}
