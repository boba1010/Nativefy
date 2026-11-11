using LibVLCSharp.Platforms.Windows;
using Nativefy.Services.Playback;

namespace Nativefy.Views;

public sealed partial class MediaPlayerPage : Page
{
    private readonly PlaybackController _controller;

    public MediaPlayerPage()
    {
        InitializeComponent();

        _controller = App.Services.GetRequiredService<PlaybackController>();

        Unloaded += (_, _) => VideoView.MediaPlayer = null;
    }

    private void VideoView_Initialized(object? sender, InitializedEventArgs e)
    {
        _controller.InitializeVideo(e.SwapChainOptions);
        VideoView.MediaPlayer = _controller.Player;
    }

    private void Surface_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_controller.IsPlaying)
            _controller.Pause();
        else
            _controller.Play();
    }
}
