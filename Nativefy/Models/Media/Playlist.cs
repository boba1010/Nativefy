namespace Nativefy.Models.Media;

public class Playlist
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<MediaItem> Items { get; set; } = [];
}
