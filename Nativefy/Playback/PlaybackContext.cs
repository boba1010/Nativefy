using Nativefy.Models.Media;

namespace Nativefy.Playback;

public sealed class PlaybackContext
{
    public MediaItem? CurrentItem { get; internal set; }

    public Playlist? Playlist { get; internal set; }

    public int PlaylistIndex { get; internal set; } = -1;

    public TimeSpan Position { get; internal set; }

    public TimeSpan Duration { get; internal set; }

    public bool IsPlaying { get; internal set; }
}