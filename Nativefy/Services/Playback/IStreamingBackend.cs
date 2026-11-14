using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace Nativefy.Services.Playback;

public interface IStreamingBackend
{
    Task<MediaSource> CreateSourceAsync(
        MediaItem item,
        Resolution resolution,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

public readonly record struct DownloadProgress(
    double Percentage,
    long DownloadedBytes,
    long? TotalBytes,
    double? BytesPerSecond,
    TimeSpan? EstimatedRemaining);