using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace Nativefy.Services.Playback;

public enum Resolution
{
    Low144p,
    Low240p,
    Standard360p,
    Standard480p,
    HD720p,
    FullHD1080p,

    QHD1440p,
}

public interface IStreamingBackend
{
    Task<MediaSource> CreateSourceAsync(
        MediaItem item,
        Resolution resolution,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<bool> Exists(MediaItem item, Resolution resolution, CancellationToken cancellationToken = default);
}

public readonly record struct DownloadProgress(
    double Percentage,
    long DownloadedBytes,
    long? TotalBytes,
    double? BytesPerSecond,
    TimeSpan? EstimatedRemaining);