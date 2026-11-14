using Nativefy.Models.Media;
using System.Threading.Tasks;

namespace Nativefy.Services.OnlineMetadata;

public interface IOnlinePlaylistService
{
    public Task<Playlist> GetPlaylistAsync(string id);
}
