using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;
using Nativefy.Services.LocalMetadata;

namespace Nativefy.ViewModels;

public sealed partial class PlaylistViewModel(IPlaylistsService playlistsService, IHistoryService history, ILikedMediaService likedMedia) : BaseViewModel
{
    [ObservableProperty]
    public partial Playlist Playlist { get; set; }

    public void LoadPlaylist(string id)
    {
        Playlist = playlistsService.GetPlaylistById(id)!;
        if (Playlist is null)
        {
            var item = history.GetById(id);
            if (item is HistoryPlaylist playlist)
            {
                Playlist = new Playlist()
                {
                    Id = playlist.Id,
                    Author = playlist.Author,
                    ImageUrl = playlist.ImageUrl,
                    Name = playlist.Title,
                    Url = playlist.Url,
                    Items = [.. playlist.Items.Select(i => new MediaItem()
                    {
                        Id = i.Id,
                        Url = i.Url,
                        Author = i.Author,
                        Duration = i.Duration,
                        ImageUrl = i.ImageUrl,
                        Position = i.Position,
                        Title = i.Title,
                        AddedDate = i.AddedDate
                    })]
                };
            }
        }
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
