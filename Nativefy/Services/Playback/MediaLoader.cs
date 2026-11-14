using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Playlist = Nativefy.Models.Media.Playlist;

namespace Nativefy.Services.Playback;

public enum MediaReportStage
{
    Loading,
    Downloading,
    DownloadProgress,
    Downloaded,
    Loaded,
    Opening,
    Opened,
    Failed,
    Cancelled
}

public sealed record MediaReportMessage(
    MediaItem Item, MediaReportStage Stage, DownloadProgress? Progress = null, Exception? Exception = null);

public sealed class MediaLoader(IStreamingBackend streaming)
{
    public async Task<MediaPlaybackItem> CreatePlaybackItemAsync(
        MediaItem item,
        IProgress<DownloadProgress>? progress = null,
        Resolution resolution = Resolution.Standard360p,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        MediaSource source = await streaming.CreateSourceAsync(item, resolution, progress, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        source.CustomProperties["Metadata"] = item;

        return new MediaPlaybackItem(source);
    }
}