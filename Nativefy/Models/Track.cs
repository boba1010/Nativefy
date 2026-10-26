using System.ComponentModel.DataAnnotations;

namespace Nativefy.Models;

public sealed class Track
{
    [Key]
    public string VideoId { get; set; } = null!; // YouTube ID as the Primary Key
    public string CustomTitle { get; set; } = null!;
    public string CustomArtist { get; set; } = null!;
    public string AlbumArtUrl { get; set; } = null!;
    public TimeSpan Duration { get; set; }
    public DateTime DateAdded { get; set; }
    public bool IsExplicit { get; set; }

    // Navigation property for relationships
    public List<PlaylistTrack> PlaylistTracks { get; set; } = null!;
}
