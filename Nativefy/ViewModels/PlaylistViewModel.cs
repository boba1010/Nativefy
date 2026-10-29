using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nativefy.Models.Media;
using Nativefy.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class PlaylistViewModel(IPlaylistsService playlistsService, ILikedMediaService likedMedia) : BaseViewModel
{
    [ObservableProperty]
    public partial Playlist Playlist { get; set; }

    public async Task LoadPlaylistAsync(string id)
    {
        Playlist = await playlistsService.GetPlaylistByIdAsync(id) ?? new();
    }

    public async Task LikeTrackAsync(string url)
    {
        var item = Playlist.Items.FirstOrDefault(i => i.Url == url) ?? throw new InvalidDataException();
        await likedMedia.AddToLikesAsync(item);
    }

    [RelayCommand]
    public async Task PlayPlaylistAsync(string id)
    {
        Debug.WriteLine("PLAYING PLAYLIST");
    }
}
