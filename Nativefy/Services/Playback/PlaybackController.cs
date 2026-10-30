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

public sealed class PlaybackController
{
    private readonly MediaLoader _loader;
    private CancellationTokenSource? _playlistLoadCts;

    public MediaPlayer Player { get; } = new();
    public MediaPlaybackList Queue { get; } = new();

    public RepeatState RepeatState { get; set; }
    public bool IsShuffleOn { get; set; }
    public bool IsPlaying { get; private set; }

    public TimeSpan Duration { get; private set; }
    public TimeSpan Position { get; private set; }

    public uint QueueCount => (uint)Queue.Items.Count;
    public uint CurrentIndex => Queue.CurrentItemIndex;

    public event Action<PlayerState>? StateChanged;
    public event Action<RepeatState>? RepeatStateChanged;
    public event Action<TimeSpan>? PositionChanged;
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

    private void Player_MediaOpened(MediaPlayer sender, object args)
    {
        Duration = sender.PlaybackSession.NaturalDuration;
    }

    private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        Debug.WriteLine($"Playback error: {args.Error}");
        Debug.WriteLine($"Extended error: {args.ExtendedErrorCode}");
        Debug.WriteLine($"Message: {args.ErrorMessage}");
    }

    private void Queue_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        Position = TimeSpan.Zero;
        Duration = TimeSpan.Zero;

        if (args.NewItem is null)
            return;

        if (args.NewItem.Source.CustomProperties.TryGetValue("Metadata", out object obj) && obj is MediaItem metadata)
            CurrentItemChanged?.Invoke(metadata);
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

    public async Task PlayPlaylistAsync(Playlist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        _playlistLoadCts?.Cancel();

        var cts = new CancellationTokenSource();
        _playlistLoadCts = cts;

        Queue.Items.Clear();
        QueueChanged?.Invoke();

        if (playlist.Items.Count == 0)
        {
            _playlistLoadCts = null;
            cts.Dispose();
            return;
        }

        try
        {
            var firstItem = await _loader.CreatePlaybackItemAsync(playlist.Items[0], cts.Token);

            cts.Token.ThrowIfCancellationRequested();

            Queue.Items.Add(firstItem);
            QueueChanged?.Invoke();

            Play();

            _ = LoadRemainingAsync(playlist, cts, 1);
        }
        catch
        {
            if (ReferenceEquals(_playlistLoadCts, cts))
                _playlistLoadCts = null;

            cts.Dispose();
            throw;
        }
    }

    private async Task LoadRemainingAsync(Playlist playlist, CancellationTokenSource cts, int startIndex)
    {
        try
        {
            await _loader.LoadRemainingAsync(
                playlist,
                Queue,
                startIndex,
                () => QueueChanged?.Invoke(),
                cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            if (ReferenceEquals(_playlistLoadCts, cts))
                _playlistLoadCts = null;

            cts.Dispose();
        }
    }

    public async Task PlayItemAsync(MediaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        ClearQueue();

        var playbackItem = await _loader.CreatePlaybackItemAsync(item);

        Queue.Items.Add(playbackItem);
        QueueChanged?.Invoke();

        Play();
    }

    public void ClearQueue()
    {
        _playlistLoadCts?.Cancel();
        _playlistLoadCts = null;

        Pause();

        Queue.Items.Clear();
        QueueChanged?.Invoke();

        Position = TimeSpan.Zero;
        Duration = TimeSpan.Zero;
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

    public void Play()
    {
        Player.Play();
        SetPlaying(true);
    }

    public void Pause()
    {
        Player.Pause();
        SetPlaying(false);
    }

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
}