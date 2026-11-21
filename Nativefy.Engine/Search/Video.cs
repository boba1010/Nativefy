using TubeSharp.Search;

namespace Nativefy.Engine.Search;

public sealed class Video : IPlayableSearchResult
{
    public string Id { get; }
    
    public string Title { get; }
    
    public string ImageUrl { get; }
    
    public string Author { get; }

    public string AuthorImageUrl { get; }

    public string Url { get; }

    public TimeSpan Duration { get; }

    public string Views { get; }

    public string PublishedTime { get; }

    public Video(VideoSearchResult video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.ThumbnailUrl;
        Url = video.Url;
        Author = video.Channel.Name;
        Views = video.Views;
        PublishedTime = video.PublishedTime;
        AuthorImageUrl = video.Channel.AvatarUrl ?? "channel.png";
        if (video.Duration is not null)
            Duration = video.Duration.Value;
    }
}
