using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

public interface IPlaylistsService
{
    public Task<List<OfflinePlaylist>> GetPlaylistsAsync();
    public Task<OfflinePlaylist?> GetPlaylistByIdAsync(string id);
    public Task CreatePlaylistAsync(OfflinePlaylist playlist);
    public Task DeletePlaylistAsync(string id);
    public Task UpdatePlaylistAsync(OfflinePlaylist playlist);
}
