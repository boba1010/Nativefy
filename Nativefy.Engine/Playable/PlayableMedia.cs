using TubeSharp.Videos;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;

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

    public PlayableMedia(YoutubeVideo video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnail ?? "video.png";
        Url = video.Url;
        Author = video.Channel.Name;
        Duration = video.Duration;
        Views = video.Views;
    }
}
