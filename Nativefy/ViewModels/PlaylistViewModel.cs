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
        Playlist.ImageUrl = "https://picsum.photos/200";
        Playlist.Items.Add(new()
        {
            Position = 1,
            Url = "youtube.com",
            ImageUrl = "https://picsum.photos/200",
            Title = "SOOOOOOOOONG",
            Author = "ME",
            Duration = new TimeSpan(1, 0, 0),
            AddedDate = DateTime.Now,
        });
        Playlist.Items.Add(new()
        {
            Position = 2,
            Url = "youtube.com",
            ImageUrl = "https://picsum.photos/200",
            Title = "SOOOOOOOOONG 1",
            Author = "ME",
            Duration = new TimeSpan(1, 0, 0),
            AddedDate = DateTime.Now,
        });
    }

    public async Task LikeTrackAsync(string url)
    {
        var item = Playlist.Items.FirstOrDefault(i => i.Url == url) ?? throw new InvalidDataException();
        await likedMedia.AddToLikesAsync(item);
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
