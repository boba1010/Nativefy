using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Models.Media;
using Nativefy.Services.Playback;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Nativefy.Playback;

public sealed partial class PlaybackMediaController(MediaLoader loader) : IDisposable
{
    private CancellationTokenSource? _loadCts;
    private bool _disposed;

    private CancellationTokenSource BeginLoad()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var previous = _loadCts;
        var current = new CancellationTokenSource();

        _loadCts = current;

        try
        {
            previous?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The previous operation completed concurrently.
        }

        return current;
    }

    private bool OwnsLoad(CancellationTokenSource cts)
    {
        return ReferenceEquals(_loadCts, cts) && !cts.IsCancellationRequested;
    }

    private void CompleteLoad(CancellationTokenSource cts)
    {
        if (ReferenceEquals(_loadCts, cts))
            _loadCts = null;

        cts.Dispose();
    }

    public async Task<MediaPlaybackItem?> PrepareAsync(MediaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var cts = BeginLoad();

        try
        {
            if (!await loader.Exists(item, Resolution.Standard360p, cts.Token))
                Report(item, MediaReportStage.Downloading);

            var playbackItem = await LoadItemAsync(item, cts.Token);

            if (!OwnsLoad(cts))
                return null;

            return playbackItem;
        }
        catch (OperationCanceledException)
        {
            Report(item, MediaReportStage.Cancelled);
            return null;
        }
        catch (Exception ex)
        {
            Report(item, MediaReportStage.Failed, exception: ex);
            throw;
        }
        finally
        {
            CompleteLoad(cts);
        }
    }

    private async Task<MediaPlaybackItem> LoadItemAsync(MediaItem item, CancellationToken cancellationToken)
    {
        Report(item, MediaReportStage.Loading);

        bool downloadStarted = false;

        var progress = new InlineProgress<DownloadProgress>(value =>
        {
            if (!downloadStarted)
            {
                downloadStarted = true;
                Report(item, MediaReportStage.Downloading);
            }

            Report(item, MediaReportStage.DownloadProgress, value);
        });

        var playbackItem = await loader.CreatePlaybackItemAsync(item, progress, cancellationToken: cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (downloadStarted)
            Report(item, MediaReportStage.Downloaded);

        Report(item, MediaReportStage.Loaded);

        return playbackItem;
    }

    public void Cancel()
    {
        var cts = _loadCts;
        _loadCts = null;

        try
        {
            cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The operation completed concurrently.
        }
    }

    private static void Report(MediaItem item, MediaReportStage stage, DownloadProgress? progress = null, Exception? exception = null)
    {
        WeakReferenceMessenger.Default.Send(new MediaReportMessage(item, stage, progress, exception));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Cancel();
    }

    private sealed class InlineProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }
}