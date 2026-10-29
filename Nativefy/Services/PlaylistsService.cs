using Microsoft.EntityFrameworkCore;
using Nativefy.DbContexts;
using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

internal sealed class PlaylistsService(AppDbContext dbContext) : IPlaylistsService
{
    private const string FallbackImageUrl = "music.png";

    public async Task CreatePlaylistAsync(LocalPlaylist playlist)
    {
        if (string.IsNullOrEmpty(playlist.Name))
            throw new InvalidDataException("Playlist Name must not be null");

        if (string.IsNullOrEmpty(playlist.ImageUrl))
            playlist.ImageUrl = FallbackImageUrl;

        if (dbContext.SavedPlaylists.Any(p => p.Id == playlist.Id))
            return;

        dbContext.SavedPlaylists.Add(playlist);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeletePlaylistAsync(string id)
    {
        var playlist = await dbContext.SavedPlaylists.FindAsync(id) 
            ?? throw new InvalidOperationException($"Playlist with Id {id} not found");
        dbContext.SavedPlaylists.Remove(playlist);
        await dbContext.SaveChangesAsync();
    }

    public async Task<LocalPlaylist?> GetPlaylistByIdAsync(string id)
    {
        var playlist = await dbContext.SavedPlaylists
            .AsNoTracking()
            .FirstOrDefaultAsync(playlist => playlist.Id == id);
        if (playlist is null)
            return null;

        return playlist;
    }

    public async Task<List<LocalPlaylist>> GetPlaylistsAsync()
    {
        var playlists = await dbContext.SavedPlaylists
            .AsNoTracking()
            .ToListAsync();

        return playlists;
    }

    public async Task UpdatePlaylistAsync(LocalPlaylist playlist)
    {
        if (string.IsNullOrEmpty(playlist.Name))
            throw new InvalidDataException("Playlist Name must not be null");

        var storedPlaylist = await dbContext.SavedPlaylists.FindAsync(playlist.Id)
            ?? throw new InvalidOperationException($"Playlist with Id {playlist.Id} not found"); ;

        storedPlaylist.Name = playlist.Name;
        storedPlaylist.ImageUrl = playlist.ImageUrl;
        storedPlaylist.UpdatedAt = DateTime.UtcNow;

        storedPlaylist.Items.Clear();
        if (playlist.Items is { Count: > 0 })
            storedPlaylist.Items.AddRange(playlist.Items);

        await dbContext.SaveChangesAsync();
    }

}
