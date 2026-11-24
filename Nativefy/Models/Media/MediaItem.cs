using Nativefy.Engine.Playable;

namespace Nativefy.Models.Media;

public sealed class MediaItem
{
    public string Id { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public int Position { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTime AddedDate { get; set; } = DateTime.Now;
    public TimeSpan Duration { get; set; }

    public MediaItem()
    {
    }

    public MediaItem(PlayableMedia media)
    {
        Id = media.Id;
        Url = media.Url;
        ImageUrl = media.ImageUrl!;
        Title = media.Title;
        Author = media.Author;
        Duration = media.Duration;
    }
}
