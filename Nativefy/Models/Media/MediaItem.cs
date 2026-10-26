using System.ComponentModel.DataAnnotations;

namespace Nativefy.Models.Media;

public sealed class MediaItem
{
    public string Url { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public int Position { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTime AddedDate { get; set; } = DateTime.Now;
    public TimeSpan Duration { get; set; }
}
