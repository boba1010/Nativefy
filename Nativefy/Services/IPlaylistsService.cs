using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

public interface IPlaylistsService
{
    public Task<List<LocalPlaylist>> GetPlaylistsAsync();
    public Task<LocalPlaylist?> GetPlaylistByIdAsync(string id);
    public Task CreatePlaylistAsync(LocalPlaylist playlist);
    public Task DeletePlaylistAsync(string id);
    public Task UpdatePlaylistAsync(LocalPlaylist playlist);
}
