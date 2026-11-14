using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Models.Media;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Nativefy.Controls;

namespace Nativefy.Services.Playback;

public enum PlayerState
{
    Playing,
    Paused
}

public sealed partial class PlaybackController
{
    private readonly MediaLoader _loader;

    private CancellationTokenSource? _loadCts;
    private MediaItem? _currentItem;

    public MediaPlayer Player { get; } = new();
    public MediaPlaybackList Queue { get; } = new();

    public RepeatState RepeatState { get; set; }
    public bool IsShuffleOn { get; private set; }
    public bool IsPlaying { get; private set; }

    public TimeSpan Duration { get; private set; }
    public TimeSpan Position { get; private set; }

    public uint QueueCount => (uint)Queue.Items.Count;
    public uint CurrentIndex => Queue.CurrentItemIndex;

    public event Action<PlayerState>? StateChanged;
    public event Action<RepeatState>? RepeatStateChanged;
    public event Action<TimeSpan>? PositionChanged;
    public event Action<TimeSpan>? DurationChanged;
    public event Action<MediaItem>? CurrentItemChanged;
    public event Action? QueueChanged;
    public event Action? SeekCompleted;

    public PlaybackController(MediaLoader loader)
    {
        _loader = loader;

        Player.CommandManager.IsEnabled = false;
        Player.Source = Queue;

        Player.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
        Player.PlaybackSession.SeekCompleted += PlaybackSession_SeekCompleted;
        Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

        Player.MediaOpened += Player_MediaOpened;
        Player.MediaFailed += Player_MediaFailed;

        Queue.CurrentItemChanged += Queue_CurrentItemChanged;
    }

    private CancellationTokenSource BeginLoad()
    {
        var previous = _loadCts;
        var current = new CancellationTokenSource();

        _loadCts = current;

        previous?.Cancel();

        return current;
    }

    private bool OwnsQueue(CancellationTokenSource cts)
    {
        return ReferenceEquals(_loadCts, cts) && !cts.IsCancellationRequested;
    }

    private void CompleteLoad(CancellationTokenSource cts)
    {
        if (ReferenceEquals(_loadCts, cts))
            _loadCts = null;

        cts.Dispose();
    }

    private void ResetQueue()
    {
        Pause();

        Queue.Items.Clear();

        _currentItem = null;
        Position = TimeSpan.Zero;
        Duration = TimeSpan.Zero;

        PositionChanged?.Invoke(Position);
        DurationChanged?.Invoke(Duration);
        QueueChanged?.Invoke();
    }

    public async Task PlayPlaylistAsync(Playlist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        var cts = BeginLoad();
        ResetQueue();

        if (playlist.Items.Count == 0)
        {
            CompleteLoad(cts);
            return;
        }

        bool handedOff = false;

        try
        {
            var firstItem = playlist.Items[0];

            var playbackItem = await LoadItemAsync(firstItem, cts.Token);

            if (!OwnsQueue(cts))
                return;

            Queue.Items.Add(playbackItem);
            QueueChanged?.Invoke();

            Play();

            handedOff = true;

            _ = LoadRemainingAsync(playlist, 1, cts);
        }
        catch (OperationCanceledException)
        {
            Report(playlist.Items[0], MediaReportStage.Cancelled);
        }
        catch (Exception ex)
        {
            Report(playlist.Items[0], MediaReportStage.Failed, exception: ex);
            throw;
        }
        finally
        {
            if (!handedOff)
                CompleteLoad(cts);
        }
    }

    private async Task LoadRemainingAsync(Playlist playlist, int startIndex, CancellationTokenSource cts)
    {
        try
        {
            for (int i = startIndex; i < playlist.Items.Count; i++)
            {
                cts.Token.ThrowIfCancellationRequested();

                if (!OwnsQueue(cts))
                    return;

                var item = playlist.Items[i];

                try
                {
                    var playbackItem = await LoadItemAsync(item, cts.Token);

                    if (!OwnsQueue(cts))
                        return;

                    Queue.Items.Add(playbackItem);
                    QueueChanged?.Invoke();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Report(item, MediaReportStage.Failed, exception: ex);
                    Debug.WriteLine(ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when another playback operation takes ownership.
        }
        finally
        {
            CompleteLoad(cts);
        }
    }

    public async Task PlayItemAsync(MediaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var cts = BeginLoad();
        ResetQueue();

        try
        {
            var playbackItem = await LoadItemAsync(item, cts.Token);

            if (!OwnsQueue(cts))
                return;

            Queue.Items.Add(playbackItem);
            QueueChanged?.Invoke();

            Play();
        }
        catch (OperationCanceledException)
        {
            Report(item, MediaReportStage.Cancelled);
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

        var playbackItem = await _loader.CreatePlaybackItemAsync(item, progress, cancellationToken: cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (downloadStarted)
            Report(item, MediaReportStage.Downloaded);

        Report(item, MediaReportStage.Loaded);

        return playbackItem;
    }

    private void Queue_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        Position = TimeSpan.Zero;
        Duration = TimeSpan.Zero;

        PositionChanged?.Invoke(Position);
        DurationChanged?.Invoke(Duration);

        _currentItem = null;

        if (args.NewItem is null)
            return;

        if (args.NewItem.Source.CustomProperties.TryGetValue("Metadata", out object obj) && obj is MediaItem metadata)
        {
            _currentItem = metadata;

            CurrentItemChanged?.Invoke(metadata);
            Report(metadata, MediaReportStage.Opening);
        }
    }

    private void Player_MediaOpened(MediaPlayer sender, object args)
    {
        Duration = sender.PlaybackSession.NaturalDuration;
        DurationChanged?.Invoke(Duration);

        if (_currentItem is not null)
            Report(_currentItem, MediaReportStage.Opened);
    }

    private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        Debug.WriteLine($"Playback error: {args.Error}");
        Debug.WriteLine($"Extended error: {args.ExtendedErrorCode}");
        Debug.WriteLine($"Message: {args.ErrorMessage}");

        if (_currentItem is not null)
            Report(_currentItem, MediaReportStage.Failed, exception: args.ExtendedErrorCode);
    }

    private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
    {
        SetPlaying(sender.PlaybackState == MediaPlaybackState.Playing);
    }

    private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
    {
        Position = sender.Position;
        PositionChanged?.Invoke(Position);
    }

    private void PlaybackSession_SeekCompleted(MediaPlaybackSession sender, object args)
    {
        SeekCompleted?.Invoke();
    }

    private void SetPlaying(bool value)
    {
        if (IsPlaying == value)
            return;

        IsPlaying = value;

        StateChanged?.Invoke(value ? PlayerState.Playing : PlayerState.Paused);
    }

    private static void Report(MediaItem item, MediaReportStage stage, DownloadProgress? progress = null, Exception? exception = null)
    {
        WeakReferenceMessenger.Default.Send(new MediaReportMessage(item, stage, progress, exception));
    }

    public void ClearQueue()
    {
        var cts = _loadCts;
        _loadCts = null;

        cts?.Cancel();

        ResetQueue();
    }

    public void ToggleRepeat()
    {
        var previousState = RepeatState;

        RepeatState = RepeatState switch
        {
            RepeatState.Off => RepeatState.RepeatAll,
            RepeatState.RepeatAll => RepeatState.RepeatOne,
            RepeatState.RepeatOne => RepeatState.Off,
            _ => RepeatState.Off
        };

        Queue.AutoRepeatEnabled = RepeatState == RepeatState.RepeatAll;

        if (previousState != RepeatState)
            RepeatStateChanged?.Invoke(RepeatState);
    }

    public void ToggleShuffle()
    {
        IsShuffleOn = !IsShuffleOn;
        Queue.ShuffleEnabled = IsShuffleOn;
    }

    public void Play() => Player.Play();

    public void Pause() => Player.Pause();

    public void TogglePlayPause()
    {
        if (IsPlaying)
            Pause();
        else
            Play();
    }

    public void Seek(TimeSpan position)
    {
        Player.Position = position;
    }

    public void SkipNext() => Queue.MoveNext();

    public void SkipPrevious() => Queue.MovePrevious();

    public void SetVolume(double volume)
    {
        Player.Volume = volume / 100.0;
    }

    private sealed class InlineProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }
}