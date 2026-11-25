using Nativefy.Engine.Channels;
using Nativefy.Engine.Playable;
using TubeSharp;

namespace Nativefy.Engine;

public sealed class ChannelEngine(YoutubeClient youtube)
{
    public async Task<Channel?> GetChannelAsync(string id, CancellationToken cancellationToken = default)
    {
        var channel = await youtube.Channels.GetChannelAsync(id, cancellationToken);

        if (channel is null)
            return null;

        return new(channel);
    }

    public async Task<ChannelVideosPage> GetVideosSectionAsync(string id, CancellationToken cancellationToken = default)
    {
        TubeSharp.Channels.ChannelVideoPage page = await youtube.Channels.GetVideosAsync(id, cancellationToken);

        return new([.. page.Videos.Select(v => new PlayableMedia(v))], page.ContinuationToken);
    }

    public async Task<ChannelVideosPage> GetNextVideosSectionAsync(string channelId, string continuationToken, CancellationToken cancellationToken = default)
    {
        TubeSharp.Channels.ChannelVideoPage page = await youtube.Channels.GetVideosNextPageAsync(channelId, continuationToken, cancellationToken);

        return new([.. page.Videos.Select(v => new PlayableMedia(v))], page.ContinuationToken);
    }

    public async Task<ChannelHomePage> GetHomeSectionAsync(string id, CancellationToken cancellationToken = default)
    {
        var home = await youtube.Channels.GetHomeAsync(id, cancellationToken);

        List<ChannelHomeShelf> shelves = [];

        foreach (var shelf in home.Shelves)
        {
            List<PlayableMedia> videos = [];

            foreach (var video in shelf.Videos)
                videos.Add(new(video));

            shelves.Add(new(shelf.Title, shelf.Subtitle, shelf.Url, videos));
        }

        return new(shelves);
    }
}