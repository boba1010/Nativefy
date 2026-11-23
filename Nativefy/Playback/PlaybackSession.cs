using Nativefy.Models.Media;
using System.Diagnostics;
using Windows.Media.Playback;

namespace Nativefy.Playback;

public sealed partial class PlaybackSession : IDisposable
{
    private bool _disposed;

    public MediaPlayer Player { get; } = new();

    public MediaPlaybackList Queue { get; } = new();

    public double Volume
    {
        get => Player.Volume;
        set => Player.Volume = value;
    }

    public bool IsPlaying => Player.PlaybackSession.PlaybackState is MediaPlaybackState.Playing;

    public TimeSpan NaturalDuration => Player.PlaybackSession.NaturalDuration;

    public event Action<MediaItem?>? CurrentItemChanged;
    public event Action? MediaOpened;
    public event Action<Exception?>? MediaFailed;
    public event Action? MediaEnded;

    public event Action<bool>? PlaybackStateChanged;
    public event Action<TimeSpan>? PositionChanged;
    public event Action<TimeSpan>? DurationChanged;
    public event Action? SeekCompleted;

    public PlaybackSession()
    {
        Player.CommandManager.IsEnabled = false;
        Player.Source = Queue;

        Player.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
        Player.PlaybackSession.SeekCompleted += PlaybackSession_SeekCompleted;
        Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

        Player.MediaOpened += Player_MediaOpened;
        Player.MediaFailed += Player_MediaFailed;
        Player.MediaEnded += Player_MediaEnded;

        Queue.CurrentItemChanged += Queue_CurrentItemChanged;
    }

    private void Queue_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
    {
        MediaItem? metadata = null;

        if (args.NewItem?.Source.CustomProperties.TryGetValue("Metadata", out object obj) == true)
            metadata = obj as MediaItem;

        CurrentItemChanged?.Invoke(metadata);
    }

    private void Player_MediaOpened(MediaPlayer sender, object args)
    {
        DurationChanged?.Invoke(sender.PlaybackSession.NaturalDuration);
        MediaOpened?.Invoke();
    }

    private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        Debug.WriteLine($"Playback error: {args.Error}");
        Debug.WriteLine($"Extended error: {args.ExtendedErrorCode}");
        Debug.WriteLine($"Message: {args.ErrorMessage}");

        MediaFailed?.Invoke(args.ExtendedErrorCode);
    }

    private void Player_MediaEnded(MediaPlayer sender, object args)
    {
        MediaEnded?.Invoke();
    }

    private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
    {
        PlaybackStateChanged?.Invoke(sender.PlaybackState is MediaPlaybackState.Playing);
    }

    private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
    {
        PositionChanged?.Invoke(sender.Position);
    }

    private void PlaybackSession_SeekCompleted(MediaPlaybackSession sender, object args)
    {
        SeekCompleted?.Invoke();
    }

    public void Enqueue(MediaPlaybackItem item) => Queue.Items.Add(item);

    public void ClearQueue() => Queue.Items.Clear();

    public void Play() => Player.Play();

    public void Pause() => Player.Pause();

    public void Seek(TimeSpan position) => Player.Position = position;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        Player.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
        Player.PlaybackSession.SeekCompleted -= PlaybackSession_SeekCompleted;
        Player.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;

        Player.MediaOpened -= Player_MediaOpened;
        Player.MediaFailed -= Player_MediaFailed;
        Player.MediaEnded -= Player_MediaEnded;

        Queue.CurrentItemChanged -= Queue_CurrentItemChanged;

        Player.Dispose();
    }
}