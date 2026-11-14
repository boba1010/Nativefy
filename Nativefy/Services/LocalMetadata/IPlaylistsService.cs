using Nativefy.Models.Media;

namespace Nativefy.Services;

public interface IPlaylistsService
{
    public List<LocalPlaylist> GetPlaylists();
    public LocalPlaylist? GetPlaylistById(string id);
    public void CreatePlaylist(LocalPlaylist playlist);
    public void DeletePlaylist(string id);
    public void UpdatePlaylist(LocalPlaylist playlist);
}
