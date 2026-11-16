using Nativefy.Engine.Channels;
using YoutubeExplode;

namespace Nativefy.Engine;

public sealed class ChannelEngine(YoutubeClient youtube)
{
    public async Task<Channel> GetChannelAsync(string id, int maxVideos = 30, CancellationToken cancellationToken = default)
    {
        var channelTask = youtube.Channels.GetAsync(id, cancellationToken).AsTask();

        var videosTask = youtube.Channels
            .GetUploadsAsync(id, cancellationToken)
            .Take(maxVideos)
            .ToListAsync(cancellationToken)
            .AsTask();

        await Task.WhenAll(channelTask, videosTask);

        return new(channelTask.Result, videosTask.Result);
    }
}
