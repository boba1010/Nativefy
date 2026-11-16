using Nativefy.Engine.Playable;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;

namespace Nativefy.Engine.Channels;

public sealed class Channel
{
    public string Id { get; }
    public string Title { get; }
    public string ImageUrl { get; }
    public string Url { get; }

    public List<PlayableMedia> Videos { get; } = [];

    public Channel(string id, string title, string imageUrl, string url)
    {
        Id = id;
        Title = title;
        ImageUrl = imageUrl;
        Url = url;
    }

    public Channel(YoutubeExplode.Channels.Channel channel, List<PlaylistVideo>? videos = null)
    {
        Id = channel.Id;
        Title = channel.Title;
        ImageUrl = channel.Thumbnails.GetWithHighestResolution().Url;
        Url = channel.Url;

        if (videos is not null)
            Videos = [.. videos.Select(v => new PlayableMedia(v))];
    }
}
