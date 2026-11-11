using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Nativefy.Engine.Playable;

public sealed class PlayableMedia : IMedia
{
    public string Id { get; }
    public string Title { get; }
    public string ImageUrl { get; }
    public string Url { get; }
    public string Author { get; }
    public TimeSpan Duration { get; }
    public List<string> Keywords { get; } = [];
    public string? Description { get; }

    public PlayableMedia(Search.Video video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.ImageUrl;
        Url = video.Url;
        Author = video.Author;
        Duration = video.Duration;
    }

    public PlayableMedia(Video video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnails.GetWithHighestResolution().Url;
        Url = video.Url;
        Author = video.Author.ChannelTitle;
        Keywords = [.. video.Keywords];
        Description = video.Description;
        if (video.Duration is not null)
            Duration = video.Duration.Value;
    }

    public PlayableMedia(PlaylistVideo video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnails.TryGetWithHighestResolution()?.Url ?? "image.png";
        Url = video.Url;
        Author = video.Author.ChannelTitle;
        if (video.Duration is not null)
            Duration = video.Duration.Value;
    }

    public PlayableMedia(string id, string title, string imageUrl, string url, string author, TimeSpan duration, string? descrption = null, List<string>? keywords = null)
    {
        Id = id;
        Title = title;
        ImageUrl = imageUrl;
        Url = url;
        Author = author;
        Duration = duration;
        Description = descrption;
        Keywords = keywords ?? [];
    }

    public string GetSearchableText()
    {
        return string.Join(" ", Title, Author, Description ?? "", string.Join(" ", Keywords)).ToLowerInvariant();
    }
}
