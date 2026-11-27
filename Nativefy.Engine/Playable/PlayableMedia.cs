using TubeSharp.Videos;

namespace Nativefy.Engine.Playable;

public sealed class PlayableMedia : IMedia
{
    public string Id { get; }

    public string Title { get; }
    
    public string? ImageUrl { get; }
    
    public string Url { get; }
    
    public string Author { get; }
    
    public TimeSpan Duration { get; }
    
    public string? Description { get; }

    public string? Views { get; }

    public PlayableMedia(YoutubeVideo video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnail ?? "image.png";
        Url = video.Url;
        Author = video.Channel.Name;
        Duration = video.Duration;
    }
}
