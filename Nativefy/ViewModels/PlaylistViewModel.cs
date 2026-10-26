using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nativefy.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class PlaylistViewModel(IPlaylistsService playlistsService) : BaseViewModel
{
    [ObservableProperty]
    public partial Playlist Playlist { get; set; }

    public async Task LoadPlaylistAsync(string id)
    {
        Playlist = await playlistsService.GetPlaylistByIdAsync(id) ?? new();
        Playlist.ImageUrl = "https://picsum.photos/200";
        Playlist.Tracks.Add(new()
        {
            Playlist = Playlist,
            PlaylistId = Playlist.Id,
            Position = 1,
            VideoId = "youtube.com",
            Track = new()
            {
                VideoId = "youtube.com",
                AlbumArtUrl = "https://picsum.photos/200",
                CustomArtist = "ME",
                CustomTitle = "SOOOOOOOOONG",
                DateAdded = DateTime.Now,
                Duration = new TimeSpan(1, 0, 0),
                PlaylistTracks = Playlist.Tracks,
            }
        });
        Playlist.Tracks.Add(new()
        {
            Playlist = Playlist,
            PlaylistId = Playlist.Id,
            Position = 2,
            VideoId = "youtube.com",
            Track = new()
            {
                VideoId = "youtube.com",
                AlbumArtUrl = "https://picsum.photos/200",
                CustomArtist = "ME",
                CustomTitle = "SOOOOOOOOONG 1",
                DateAdded = DateTime.Now,
                Duration = new TimeSpan(1, 30, 0),
                PlaylistTracks = Playlist.Tracks,
            }
        });
    }

    public async Task LikeTrackAsync(string id)
    {

    }

    [RelayCommand]
    public async Task LikePlaylistAsync(string id)
    {
        Debug.WriteLine("LIKE CLICKED!");
    }

    [RelayCommand]
    public async Task PlayPlaylistAsync(string id)
    {
        Debug.WriteLine("PLAYING PLAYLIST");
    }
}
