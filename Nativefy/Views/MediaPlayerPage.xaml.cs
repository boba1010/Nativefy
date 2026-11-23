using Nativefy.Playback;

namespace Nativefy.Views;

public sealed partial class MediaPlayerPage : Page
{
    private readonly PlaybackController _controller;

    public MediaPlayerPage()
    {
        InitializeComponent();

        _controller = App.Services.GetRequiredService<PlaybackController>();

        MediaPlayerElement.SetMediaPlayer(_controller.Session.Player);
    }

    private void MediaPlayerElement_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_controller.Context.IsPlaying)
            _controller.Pause();
        else
            _controller.Play();
    }
}
