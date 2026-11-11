using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;

namespace Nativefy.Services.Playback;

public sealed class StreamingBackend(NativefyEngine nativefy) : IStreamingBackend
{
    public async Task<Resolution[]> GetResolutionsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await nativefy.Streaming.GetResolutionsAsync(id, cancellationToken);
    }

    public async Task<StreamUrls> GetStreamUrlsAsync(MediaItem item, Resolution resolution, CancellationToken cancellationToken = default)
    {
        var (video, audio) =await nativefy.Streaming.GetPlaybackUrlsAsync(item.Id, resolution);

        return new(video, audio);
    }
}