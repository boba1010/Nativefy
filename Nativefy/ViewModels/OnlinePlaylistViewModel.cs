using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;
using Nativefy.Services.OnlineMetadata;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class OnlinePlaylistViewModel(IOnlinePlaylistService onlinePlaylist, IPlaylistsService playlists, ILikedMediaService likedMedia) : BaseViewModel
{
    [ObservableProperty]
    public partial string Id { get; set; } = null!;

    [ObservableProperty]
    public partial string Title { get; set; } = null!;

    [ObservableProperty]
    public partial string ImageUrl { get; set; } = "image.png";

    [ObservableProperty]
    public partial int ItemsCount { get; set; }

    [ObservableProperty]
    public partial string FormattedDuration { get; set; }

    public string Url { get; set; } = null!;

    public string Author { get; set; } = null!;

    public ObservableCollection<MediaItem> Items { get; } = [];
    public ObservableCollection<byte> DummyItems { get; } = [.. Enumerable.Repeat((byte)0, 100)];

    public async Task LoadPlaylistAsync(string id)
    {
        IsLoading = true;

        try
        {
            var playlist = await onlinePlaylist.GetPlaylistAsync(id);

            Id = playlist.Id;
            Title = playlist.Title;
            ImageUrl = playlist.ImageUrl;

            foreach (var item in playlist.Items)
                Items.Add(item);

            ItemsCount = Items.Count;
            FormattedDuration = FormatTotalDuration();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string FormatTotalDuration()
    {
        var hours = 0;
        var minutes = 0;

        foreach (var item in Items)
        {
            hours += item.Duration.Hours;
            minutes += item.Duration.Minutes;
        }

        if (hours >= 1)
            return $"{hours} hr {minutes} min";

        return $"{minutes} min";
    }

    [RelayCommand]
    public void PlayPlaylist()
    {
        WeakReferenceMessenger.Default.Send(new PlayPlaylistMessage(new()
        {
            Id = Id,
            Items = [.. Items],
            ImageUrl = ImageUrl,
            Title = Title,
        }));
    }

    public void PlayItem(MediaItem item)
    {
        WeakReferenceMessenger.Default.Send(new PlayItemMessage(item));
    }

    [RelayCommand]
    public async Task LikePlaylistAsync()
    {
        playlists.CreatePlaylist(new()
        {
            Id = Id,
            Title = Title,
            ImageUrl = ImageUrl,
            Author = Author,
            Url = Url,
            Items = [.. Items],
        });

        WeakReferenceMessenger.Default.Send(new RefreshPlaylistsMessage());
    }

    public async Task LikeItemAsync(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id) ?? throw new InvalidDataException();
        likedMedia.AddToLikes(item);
    }
}
