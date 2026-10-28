using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nativefy.Models.Media;
using Nativefy.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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

    public async Task LoadLikedMediaPlaylistAsync()
    {
        Playlist = await likedMedia.GetLikedMediaAsync() ?? new();

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

    [RelayCommand]
    public async Task PlayPlaylistAsync()
    {

    }
}
