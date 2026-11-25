using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Controls;
using Nativefy.Models.Media;
using Nativefy.Services.Playback;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI;

namespace Nativefy.Playback;

public enum PlayerState
{
    Playing,
    Paused
}

public sealed partial class PlaybackController : IDisposable
{
    private readonly PlaybackMediaController _mediaController;
    private readonly PlaylistNavigator _navigator = new();
    
    public PlaybackSession Session { get; } = new();

    public PlaybackContext Context { get; } = new();

    public RepeatState RepeatState
    {
        get => _navigator.RepeatState;
        private set => _navigator.RepeatState = value;
    }

    public bool IsShuffleOn => _navigator.ShuffleEnabled;

    public uint QueueCount => (uint)(Context.Playlist?.Items.Count ?? Session.Queue.Items.Count);

    public uint CurrentIndex => Context.Playlist is null ? 0 : (uint)Math.Max(0, Context.PlaylistIndex);

    public event Action<PlayerState>? StateChanged;
    public event Action<RepeatState>? RepeatStateChanged;
    public event Action<TimeSpan>? PositionChanged;
    public event Action<TimeSpan>? DurationChanged;
    public event Action<MediaItem>? CurrentItemChanged;
    public event Action? QueueChanged;
    public event Action? SeekCompleted;

    public PlaybackController(PlaybackMediaController mediaController)
    {
        _mediaController = mediaController;

        Session.CurrentItemChanged += OnCurrentItemChanged;
        Session.MediaOpened += OnMediaOpened;
        Session.MediaFailed += OnMediaFailed;
        Session.MediaEnded += OnMediaEnded;

        Session.PlaybackStateChanged += OnPlaybackStateChanged;
        Session.PositionChanged += OnPositionChanged;
        Session.DurationChanged += OnDurationChanged;
        Session.SeekCompleted += OnSeekCompleted;
    }

    private void OnCurrentItemChanged(MediaItem? item)
    {
        Context.Position = TimeSpan.Zero;
        Context.Duration = TimeSpan.Zero;
        Context.CurrentItem = item;

        PositionChanged?.Invoke(Context.Position);
        DurationChanged?.Invoke(Context.Duration);

        if (item is null)
            return;

        CurrentItemChanged?.Invoke(item);
        Report(item, MediaReportStage.Opening);
    }

    private void OnMediaOpened()
    {
        OnDurationChanged(Session.NaturalDuration);

        if (Context.CurrentItem is not null)
            Report(Context.CurrentItem, MediaReportStage.Opened);
    }

    private void OnMediaFailed(Exception? exception)
    {
        if (Context.CurrentItem is not null)
            Report(Context.CurrentItem, MediaReportStage.Failed, exception: exception);
    }

    private static void Report(MediaItem item, MediaReportStage stage, Exception? exception = null)
    {
        WeakReferenceMessenger.Default.Send(new MediaReportMessage(item, stage, Exception: exception));
    }

    private async void OnMediaEnded()
    {
        Debug.WriteLine($"MediaEnded: Repeat={RepeatState}, Playlist={Context.Playlist is not null}");

        if (Context.Playlist is null)
        {
            if (RepeatState is RepeatState.RepeatOne)
                Play();
            return;
        }

        if (RepeatState is RepeatState.RepeatOne)
        {
            await SelectPlaylistItemAsync(Context.PlaylistIndex);
            return;
        }

        await SkipNextAsync();
    }

    private void OnPlaybackStateChanged(bool isPlaying)
    {
        SetPlaying(isPlaying);
    }

    private void OnPositionChanged(TimeSpan position)
    {
        Context.Position = position;
        PositionChanged?.Invoke(position);
    }

    private void OnDurationChanged(TimeSpan duration)
    {
        Context.Duration = duration;
        DurationChanged?.Invoke(duration);
    }

    private void OnSeekCompleted()
    {
        SeekCompleted?.Invoke();
    }

    private void ResetQueue()
    {
        Pause();

        Session.ClearQueue();
        
        Context.CurrentItem = null;

        Context.Position = TimeSpan.Zero;
        Context.Duration = TimeSpan.Zero;

        PositionChanged?.Invoke(Context.Position);
        DurationChanged?.Invoke(Context.Duration);
        QueueChanged?.Invoke();
    }

    public async Task PlayPlaylistAsync(Playlist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        _navigator.Reset();

        Context.Playlist = playlist;
        Context.PlaylistIndex = 0;

        if (playlist.Items.Count == 0)
        {
            ClearQueue();
            return;
        }

        await SelectPlaylistItemAsync(0);
    }

    public async Task PlayItemAsync(MediaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _navigator.Reset();

        Context.Playlist = null;
        Context.PlaylistIndex = -1;

        await LoadAndPlayAsync(item);
    }

    public void CancelDownload()
    {
        _mediaController.Cancel();
    }

    private async Task SelectPlaylistItemAsync(int index)
    {
        var playlist = Context.Playlist;

        if (playlist is null || index < 0 || index >= playlist.Items.Count)
            return;

        Context.PlaylistIndex = index;

        await LoadAndPlayAsync(playlist.Items[index]);
    }

    private async Task LoadAndPlayAsync(MediaItem item)
    {
        ResetQueue();

        MediaPlaybackItem? playbackItem;

        try
        {
            playbackItem = await _mediaController.PrepareAsync(item);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Playback preparation failed: {exception}");
            return;
        }

        if (playbackItem is null)
            return;

        Session.Enqueue(playbackItem);
        QueueChanged?.Invoke();

        Play();
    }

    private void SetPlaying(bool value)
    {
        if (Context.IsPlaying == value)
            return;

        Context.IsPlaying = value;

        StateChanged?.Invoke(value ? PlayerState.Playing : PlayerState.Paused);
    }

    public void ClearQueue()
    {
        _mediaController.Cancel();
        _navigator.Reset();

        Context.Playlist = null;
        Context.PlaylistIndex = -1;

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

        Session.Queue.AutoRepeatEnabled = false;

        if (previousState != RepeatState)
            RepeatStateChanged?.Invoke(RepeatState);
    }

    public void ToggleShuffle()
    {
        _navigator.ShuffleEnabled = !_navigator.ShuffleEnabled;
        _navigator.Reset();

        // Navigation is handled by the controller.
        Session.Queue.ShuffleEnabled = false;
    }

    public void Play() => Session.Play();

    public void Pause() => Session.Pause();

    public void TogglePlayPause()
    {
        if (Context.IsPlaying)
            Pause();
        else
            Play();
    }

    public void Seek(TimeSpan position)
    {
        Session.Seek(position);
    }

    public void SkipNext()
    {
        _ = SkipNextAsync();
    }

    public async Task SkipNextAsync()
    {
        var nextIndex = _navigator.GetNextIndex(Context.Playlist, Context.PlaylistIndex);
        if (nextIndex < 0)
            return;

        await SelectPlaylistItemAsync(nextIndex);
    }

    public void SkipPrevious()
    {
        _ = SkipPreviousAsync();
    }

    public async Task SkipPreviousAsync()
    {
        var previousIndex = _navigator.GetPreviousIndex(Context.Playlist, Context.PlaylistIndex);
        if (previousIndex < 0)
            return;

        await SelectPlaylistItemAsync(previousIndex);
    }

    public void SetVolume(double volume)
    {
        Session.Volume = volume / 100.0;
    }

    public void Dispose()
    {
        Session.CurrentItemChanged -= OnCurrentItemChanged;
        Session.MediaOpened -= OnMediaOpened;
        Session.MediaFailed -= OnMediaFailed;
        Session.MediaEnded -= OnMediaEnded;

        Session.PlaybackStateChanged -= OnPlaybackStateChanged;
        Session.PositionChanged -= OnPositionChanged;
        Session.DurationChanged -= OnDurationChanged;
        Session.SeekCompleted -= OnSeekCompleted;

        _mediaController.Dispose();
        Session.Dispose();
    }
}