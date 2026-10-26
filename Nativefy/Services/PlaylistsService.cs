using Microsoft.EntityFrameworkCore;
using Nativefy.DbContexts;
using System.Threading.Tasks;

namespace Nativefy.Services;

internal sealed class PlaylistsService(AppDbContext dbContext) : IPlaylistsService
{
    private const string FallbackImageUrl = "music.svg";

    public async Task CreatePlaylistAsync(Playlist playlist)
    {
        if (string.IsNullOrEmpty(playlist.Name))
            throw new InvalidDataException("Playlist Name must not be null");

        if (string.IsNullOrEmpty(playlist.ImageUrl))
            playlist.ImageUrl = FallbackImageUrl;

        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeletePlaylistAsync(string id)
    {
        var playlist = await dbContext.Playlists.FindAsync(id) 
            ?? throw new InvalidOperationException($"Playlist with Id {id} not found");
        dbContext.Playlists.Remove(playlist);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Playlist?> GetPlaylistByIdAsync(string id)
    {
        var playlist = await dbContext.Playlists
            .AsNoTracking()
            .Include(p => p.Tracks)
                .ThenInclude(pt => pt.Track)
            .FirstOrDefaultAsync(playlist => playlist.Id == id);
        if (playlist is null)
            return null;

        return playlist;
    }

    public async Task<List<Playlist>> GetPlaylistsAsync()
    {
        var playlists = await dbContext.Playlists
            .AsNoTracking()
            .Include(p => p.Tracks)
                .ThenInclude(pt => pt.Track)
            .ToListAsync();
        
        return playlists;
    }

    public async Task UpdatePlaylistAsync(Playlist playlist)
    {
        if (string.IsNullOrEmpty(playlist.Name))
            throw new InvalidDataException("Playlist Name must not be null");

        var storedPlaylist = await dbContext.Playlists.FindAsync(playlist.Id)
            ?? throw new InvalidOperationException($"Playlist with Id {playlist.Id} not found"); ;

        storedPlaylist.Name = playlist.Name;
        storedPlaylist.ImageUrl = playlist.ImageUrl;
        storedPlaylist.Tracks = playlist.Tracks;

        await dbContext.SaveChangesAsync();
    }

}
