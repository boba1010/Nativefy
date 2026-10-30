using Nativefy.Services.Playback;

namespace Nativefy.Views;

public sealed partial class MediaPlayerPage : Page
{
    private readonly PlaybackController _controller;

    public MediaPlayerPage()
    {
        InitializeComponent();

        _controller = App.Services.GetRequiredService<PlaybackController>();

        MediaPlayerElement.SetMediaPlayer(_controller.Player);
    }
}
