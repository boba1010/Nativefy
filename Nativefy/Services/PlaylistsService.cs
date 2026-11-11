using Nativefy.DbContexts;
using Nativefy.Models.Media;

namespace Nativefy.Services;

internal sealed class PlaylistsService : IPlaylistsService
{
    private const string FallbackImageUrl = "music.png";

    public void CreatePlaylist(LocalPlaylist playlist)
    {
        if (string.IsNullOrEmpty(playlist.Name))
            throw new InvalidDataException("Playlist Name must not be null");

        if (string.IsNullOrEmpty(playlist.ImageUrl))
            playlist.ImageUrl = FallbackImageUrl;

        if (NativefyDatabaseProvider.Playlists.Any(p => p.Id == playlist.Id))
            return;

        NativefyDatabaseProvider.Playlists.Add(playlist);
        NativefyDatabaseProvider.SaveChanges();
    }

    public void DeletePlaylist(string id)
    {
        var playlist = NativefyDatabaseProvider.Playlists.FirstOrDefault(p => p.Id == id)
            ?? throw new InvalidOperationException($"Playlist with Id {id} not found");
        NativefyDatabaseProvider.Playlists.Remove(playlist);
        NativefyDatabaseProvider.SaveChanges();
    }

    public LocalPlaylist? GetPlaylistById(string id)
    {
        var playlist = NativefyDatabaseProvider.Playlists.FirstOrDefault(playlist => playlist.Id == id);
        if (playlist is null)
            return null;

        return playlist;
    }

    public List<LocalPlaylist> GetPlaylists()
    {
        var playlists = NativefyDatabaseProvider.Playlists.ToList();
        return playlists;
    }

    public void UpdatePlaylist(LocalPlaylist playlist)
    {
        if (string.IsNullOrEmpty(playlist.Name))
            throw new InvalidDataException("Playlist Name must not be null");

        var storedPlaylist = NativefyDatabaseProvider.Playlists.FirstOrDefault(p => p.Id == playlist.Id)
            ?? throw new InvalidOperationException($"Playlist with Id {playlist.Id} not found");

        storedPlaylist.Name = playlist.Name;
        storedPlaylist.ImageUrl = playlist.ImageUrl;
        storedPlaylist.Author = playlist.Author;
        storedPlaylist.Url = playlist.Url;
        storedPlaylist.UpdatedAt = DateTime.UtcNow;

        storedPlaylist.Items.Clear();
        if (playlist.Items is { Count: > 0 })
            storedPlaylist.Items.AddRange(playlist.Items);

        NativefyDatabaseProvider.SaveChanges();
    }

}
