using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services;

public interface IOnlinePlaylistService
{
    public Task<Playlist> GetPlaylistAsync(string id);
}
