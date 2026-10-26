using System.ComponentModel.DataAnnotations;

namespace Nativefy.Models;

public sealed class Playlist
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public List<PlaylistTrack> Tracks { get; set; } = [];
}
