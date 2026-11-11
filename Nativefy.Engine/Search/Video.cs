using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace Nativefy.Engine.Search;

public sealed class Video : ISearchResult
{
    public string Id { get; }
    
    public string Title { get; }
    
    public string ImageUrl { get; }
    
    public string Author { get; }

    public string Url { get; }

    public TimeSpan Duration { get; }

    public Video(VideoSearchResult video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnails.GetWithHighestResolution().Url;
        Url = video.Url;
        Author = video.Author.ChannelTitle;
        if (video.Duration is not null)
            Duration = video.Duration.Value;
    }
}
