namespace Nativefy.Engine;

/// <summary>
/// A flat data model representing either a video or a playlist inside the SQLite JSON payload.
/// </summary>
public sealed class PlayableItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public TimeSpan? Duration { get; set; }
    public bool IsPlaylist { get; set; }

    // Parameterless constructor for JSON deserialization
    public PlayableItemDto() { }

    // Converts an existing runtime item into a flat DB record
    public PlayableItemDto(IPlayableItem item)
    {
        Id = item.Id;
        Title = item.Title;
        ImageUrl = item.ImageUrl;
        Author = item.Author;
        Url = item.Url;
        Duration = item.Duration;
        IsPlaylist = item.IsPlaylist;
    }

    /// <summary>
    /// Restores the item to its true runtime wrapper class.
    /// </summary>
    public IPlayableItem ToRuntimeItem()
    {
        return IsPlaylist ? new PlayablePlaylist(this) : new PlayableMedia(this);
    }
}
