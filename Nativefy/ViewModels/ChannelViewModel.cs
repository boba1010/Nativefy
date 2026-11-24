using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Engine;
using Nativefy.Engine.Channels;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services.LocalMetadata;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public enum Tab
{
    Home, Videos, Playlists
}

public sealed partial class ChannelViewModel(NativefyEngine nativefy, ILikedChannelsService likedChannels) : BaseViewModel
{
    private string? _token;

    public string Id { get; set; } = null!;

    public string Url { get; set; } = null!;

    [ObservableProperty]
    public partial string Title { get; set; } = null!;

    [ObservableProperty]
    public partial string ImageUrl { get; set; } = "ms-appx:///avatar.png";

    [ObservableProperty]
    public partial string BannerUrl { get; set; } = "ms-appx:///banner.png";

    [ObservableProperty]
    public partial string Description { get; set; }
    
    [ObservableProperty]
    public partial string VideoCount { get; set; }

    [ObservableProperty]
    public partial string Subscribers { get; set; }

    [ObservableProperty]
    public partial string Handle { get; set; }

    [ObservableProperty]
    public partial bool IsChannelLiked { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    public partial bool IsMoreAvailable { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMoreNotLoading))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    public partial bool IsMoreLoading { get; set; }

    public bool IsMoreNotLoading => !IsMoreLoading;

    public bool CanLoadMore => IsMoreAvailable && !IsMoreLoading;

    [ObservableProperty]
    public partial ChannelTab SelectedTab { get; set; }

    [ObservableProperty]
    public partial Tab Tab { get; set; }

    public ObservableCollection<MediaItem> Videos { get; } = [];

    public ObservableCollection<ChannelHomeShelf> Shelves { get; } = [];

    //public ObservableCollection<ChannelPlaylistsPage> Playlists { get; } = [];

    public ObservableCollection<byte> DummyVideos { get; } = [.. Enumerable.Repeat((byte)0, 100)];

    public ObservableCollection<ChannelTab> Tabs { get; } = [];

    partial void OnSelectedTabChanged(ChannelTab value)
    {
        _ = LoadCurrentTabAsync();
    }

    public async Task LoadChannelAsync(string id)
    {
        IsLoading = true;

        try
        {
            var channel = await nativefy.Channel.GetChannelAsync(id);
            if (channel is null)
                return;

            Id = channel.Id;
            Title = channel.Title;
            ImageUrl = channel.ImageUrl;
            Url = channel.Url;
            BannerUrl = channel.BannerUrl ?? "ms-appx:///banner.png";
            Description = channel.Description ?? "";
            Subscribers = channel.Subscribers ?? "";
            VideoCount = channel.VideoCount ?? "";
            Handle = channel.Handle;

            foreach (var tab in channel.Tabs)
                Tabs.Add(tab);

            if (channel.Tabs.Count > 0)
            {
                var tab = channel.Tabs[0];
                tab.IsSelected = true;
                SelectedTab = tab;
            }

            IsChannelLiked = likedChannels.Exists(id);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadCurrentTabAsync()
    {
        Videos.Clear();
        Shelves.Clear();
        IsMoreLoading = true;

        try
        {
            switch (SelectedTab.Title)
            {
                case "Home":
                    {
                        Tab = Tab.Home;

                        IsMoreAvailable = false;

                        var page = await nativefy.Channel.GetHomeSectionAsync(Id);

                        foreach (var shelf in page.Shelves)
                        {
                            if (shelf.Title is "Featured Channels" or "Upcoming live streams" or "Posts" ||
                                shelf.Title.EndsWith("Channels") ||
                                shelf.Title.EndsWith("playlists", StringComparison.InvariantCultureIgnoreCase))
                                continue;

                            Shelves.Add(shelf);
                        }

                        break;
                    }

                case "Videos":
                    {
                        Tab = Tab.Videos;

                        var page = await nativefy.Channel.GetVideosSectionAsync(Id);
                        if (!string.IsNullOrEmpty(page.ContinuationToken))
                        {
                            _token = page.ContinuationToken;
                            IsMoreAvailable = true;
                        }
                        else
                            IsMoreAvailable = false;

                        foreach (var video in page.Videos)
                            Videos.Add(new MediaItem(video));

                        break;
                    }
            }
        }
        finally
        {
            IsMoreLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadMoreAsync()
    {
        IsMoreLoading = true;

        try
        {
            if (string.IsNullOrEmpty(_token))
            {
                IsMoreAvailable = false;
                return;
            }

            var page = await nativefy.Channel.GetNextVideosSectionAsync(Id, _token);
            if (!string.IsNullOrEmpty(page.ContinuationToken))
            {
                _token = page.ContinuationToken;
                IsMoreAvailable = true;
            }
            else
                IsMoreAvailable = false;

            foreach (var video in page.Videos)
                Videos.Add(new MediaItem(video));
        }
        finally
        {
            IsMoreLoading = false;
        }
    }

    public void Play(string id)
    {
        var video = Videos.FirstOrDefault(v => v.Id == id);
        if (video is null)
            return;

        WeakReferenceMessenger.Default.Send(new PlayItemMessage(video));
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
