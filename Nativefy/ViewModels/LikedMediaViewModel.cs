using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;
using System.Collections.ObjectModel;

namespace Nativefy.ViewModels;

public sealed partial class LikedMediaViewModel(ILikedMediaService likedMedia) : BaseViewModel
{
    [ObservableProperty]
    public partial LikedMediaPlaylist Playlist { get; set; }

    [ObservableProperty]
    public partial int ItemsCount { get; set; }

    [ObservableProperty]
    public partial string FormattedDuration { get; set; }

    public ObservableCollection<MediaItem> Items { get; } = [];

    public void LoadLikedMediaPlaylist()
    {
        Playlist = likedMedia.GetLikedMedia() ?? new();

        foreach (var item in Playlist.Items)
            Items.Add(item);

        ItemsCount = Items.Count;
        FormattedDuration = FormatTotalDuration();
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

    public void UnlikeMedia(string id)
    {
        likedMedia.RemoveFromLiked(id);
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is not null)
            Items.Remove(item);
    }

    [RelayCommand]
    public void PlayPlaylist()
    {
        WeakReferenceMessenger.Default.Send(new PlayPlaylistMessage(new()
        {
            Id = Playlist.Id,
            Author = Playlist.Author,
            CreatedAt = Playlist.CreatedAt,
            ImageUrl = Playlist.ImageUrl,
            Items = Playlist.Items,
            Title = Playlist.Title,
            UpdatedAt = Playlist.UpdatedAt,
            Url = Playlist.Url
        }));
    }
}
