using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;
using System.Collections.ObjectModel;

namespace Nativefy.ViewModels;

public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IPlaylistsService playlistsService;

    public ObservableCollection<SidebarMenuItem> MenuItems { get; } = [];

    [ObservableProperty]
    public partial bool IsPlayerbarVisible { get; set; } = false;

    public ShellViewModel(IPlaylistsService playlistsService)
    {
        this.playlistsService = playlistsService;

        WeakReferenceMessenger.Default.Register<PlayerBarVisiblityChangedMessage>(this, (r, m) => IsPlayerbarVisible = m.IsVisible);
    }

    public void LoadPlaylists()
    {
        MenuItems.Clear();
        var playlists = playlistsService.GetPlaylists();

        foreach (var playlist in playlists)
        {
            MenuItems.Add(new()
            {
                Text = playlist.Title,
                ImageUrl = playlist.ImageUrl,
                Tag = playlist.Id,
            });
        }
    }

    public void CreatePlaylist(LocalPlaylist playlist)
    {
        playlistsService.CreatePlaylist(playlist);
    }

    public void DeletePlaylist(string id)
    {
        playlistsService.DeletePlaylist(id);
    }

    public void RenamePlaylist(string id, string newTitle)
    {
        newTitle = newTitle.Trim();

        if (string.IsNullOrEmpty(newTitle))
            return;

        var playlist = playlistsService.GetPlaylistById(id);
        if (playlist is null)
            return;

        playlist.Title = newTitle;

        playlistsService.UpdatePlaylist(playlist);
    }
}