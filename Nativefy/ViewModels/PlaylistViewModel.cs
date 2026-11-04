using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;

namespace Nativefy.ViewModels;

public sealed partial class PlaylistViewModel(IPlaylistsService playlistsService, ILikedMediaService likedMedia) : BaseViewModel
{
    [ObservableProperty]
    public partial Playlist Playlist { get; set; }

    public void LoadPlaylist(string id)
    {
        Playlist = playlistsService.GetPlaylistById(id) ?? new();
    }

    public void LikeTrack(string id)
    {
        var item = Playlist.Items.FirstOrDefault(i => i.Id == id) ?? throw new InvalidDataException();
        likedMedia.AddToLikes(item);
    }

    [RelayCommand]
    public void PlayPlaylist(string id)
    {
        WeakReferenceMessenger.Default.Send(new PlayPlaylistMessage(Playlist));
    }

    public void PlayItem(MediaItem item)
    {
        WeakReferenceMessenger.Default.Send(new PlayItemMessage(item));
    }
}
