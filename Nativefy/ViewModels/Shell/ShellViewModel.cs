using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
                Text = playlist.Name,
                ImageUrl = playlist.ImageUrl,
                Tag = playlist.Id,
            });
        }
    }

    public async Task CreatePlaylistAsync()
    {
        var response = await WeakReferenceMessenger.Default.Send(new OpenNewPlaylistDialogMessage("New playlist", "Create", "Cancel"));

        if (response.Result != DialogResult.Primary || 
            response.GetData<LocalPlaylist>() is not { Name.Length: > 0 } created)
            return;

        playlistsService.CreatePlaylist(created);
        LoadPlaylists();
    }
}