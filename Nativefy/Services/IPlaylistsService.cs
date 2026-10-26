using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

public interface IPlaylistsService
{
    public Task<List<Playlist>> GetPlaylistsAsync();
    public Task<Playlist?> GetPlaylistByIdAsync(string id);
    public Task CreatePlaylistAsync(Playlist playlist);
    public Task DeletePlaylistAsync(string id);
    public Task UpdatePlaylistAsync(Playlist playlist);
}
