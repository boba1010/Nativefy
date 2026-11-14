using System.Text.Json.Serialization;

namespace Nativefy.Models.Media;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SearchHistory), "search")]
[JsonDerivedType(typeof(HistoryMediaItem), "media")]
[JsonDerivedType(typeof(HistoryPlaylist), "playlist")]
public abstract class HistoryItem
{
    public string Id { get; set; } = null!;
    public DateTime AddedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public sealed class SearchHistory : HistoryItem
{
    public string Query { get; set; } = null!;
}

public abstract class HistoryPlayable : HistoryItem
{
    public string Url { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
}

public sealed class HistoryMediaItem : HistoryPlayable
{
    public HistoryMediaItem() { }
    public HistoryMediaItem(string id, string url, string imageUrl, string title, string author, TimeSpan duration, int position = -1)
    {
        Id = id;
        Url = url;
        ImageUrl = imageUrl;
        Title = title;
        Author = author;
        Duration = duration;
        Position = position;
    }

    public int Position { get; set; } = -1;
    public TimeSpan Duration { get; set; }
}

public sealed class HistoryPlaylist : HistoryPlayable
{
    public List<HistoryMediaItem> Items { get; set; } = [];
}