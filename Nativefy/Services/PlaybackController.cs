using Nativefy.Controls;
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Nativefy.Services;

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
    public bool IsPlaying { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan Position { get; set; }
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
        Player.Source = Queue;

        Player.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
        Player.PlaybackSession.SeekCompleted += PlaybackSession_SeekCompleted;
        Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        Queue.CurrentItemChanged += Queue_CurrentItemChanged;
    }

    private void Queue_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        if (args.NewItem is null)
            return;

        if (args.NewItem.Source.CustomProperties.TryGetValue("Metadata", out object obj) && obj is MediaItem metadata)
            CurrentItemChanged?.Invoke(metadata);
    }

    private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
    {
        if (sender.PlaybackState == MediaPlaybackState.Paused)
        {
            StateChanged?.Invoke(PlayerState.Paused);
            IsPlaying = false;
        }
        else if (sender.PlaybackState == MediaPlaybackState.Playing)
        {
            StateChanged?.Invoke(PlayerState.Playing);
            IsPlaying = true;
        }
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

    public async Task PlayPlaylistAsync(Playlist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        _playlistLoadCts?.Cancel();

        var cts = new CancellationTokenSource();
        _playlistLoadCts = cts;

        Queue.Items.Clear();
        QueueChanged?.Invoke();

        if (playlist.Items.Count == 0)
            return;

        try
        {
            // Resolve the first track.
            var firstItem = await _loader.CreatePlaybackItemAsync(playlist.Items[0], cts.Token);

            cts.Token.ThrowIfCancellationRequested();

            // Start playback immediately.
            Queue.Items.Add(firstItem);
            QueueChanged?.Invoke();

            Play();

            // Continue loading the rest in the background.
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
            await _loader.LoadRemainingAsync(playlist, Queue, startIndex, () => QueueChanged?.Invoke(), cts.Token);
        }
        catch (OperationCanceledException)
        {
            // A newer playback request replaced this playlist.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
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

        Queue.Items.Clear();
        QueueChanged?.Invoke();
    }

    public void ToggleRepeat()
    {
        var previousState = RepeatState;

        switch (RepeatState)
        {
            case RepeatState.Off:
                RepeatState = RepeatState.RepeatAll;
                break;
            case RepeatState.RepeatAll:
                RepeatState = RepeatState.RepeatOne;
                break;
            case RepeatState.RepeatOne:
                RepeatState = RepeatState.Off;
                break;
        }

        switch (RepeatState)
        {
            case RepeatState.Off:
                Queue.AutoRepeatEnabled = false;
                Player.IsLoopingEnabled = false;
                break;

            case RepeatState.RepeatAll:
                Queue.AutoRepeatEnabled = true;  // Loops whole list
                Player.IsLoopingEnabled = false;   // Prevents trapping on 1 track
                break;

            case RepeatState.RepeatOne:
                Queue.AutoRepeatEnabled = false; // Disables list auto-advance loop
                Player.IsLoopingEnabled = true;    // Traps and loops current track
                break;
        }

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
    }

    public void Pause()
    {
        Player.Pause();
    }

    public void TogglePlayPause()
    {
        if (Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            Pause();
        else
            Play();
    }

    public void SkipNext()
    {
        Queue.MoveNext();
    }

    public void SkipPrevious()
    {
        Queue.MovePrevious();
    }

    public void Seek(TimeSpan position)
    {
        Player.PlaybackSession.Position = position;
    }

    public void SetVolume(double volume)
    {
        Player.Volume = volume / 100.0;
    }
}
