using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;
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

    public async Task LikeTrackAsync(string id)
    {
        var item = Playlist.Items.FirstOrDefault(i => i.Id == id) ?? throw new InvalidDataException();
        await likedMedia.AddToLikesAsync(item);
    }

    [RelayCommand]
    public async Task PlayPlaylistAsync(string id)
    {
        WeakReferenceMessenger.Default.Send(new PlayPlaylistMessage(Playlist));
    }

    public void PlayItem(MediaItem item)
    {
        WeakReferenceMessenger.Default.Send(new PlayItemMessage(item));
    }
}
