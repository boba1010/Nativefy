namespace Nativefy.Models;

public sealed class PlaylistTrack
{
    public string PlaylistId { get; set; } = null!;
    public Playlist Playlist { get; set; } = null!;

    public string VideoId { get; set; } = null!;
    public Track Track { get; set; } = null!;

    public int Position { get; set; } // Keeps tracks in the user's specific order
}
