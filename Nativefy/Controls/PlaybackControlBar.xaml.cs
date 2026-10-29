using Nativefy.ViewModels.Shell;

namespace Nativefy.Controls;

public sealed partial class PlaybackControlBar : UserControl
{
    public PlayerBarViewModel ViewModel { get; set; }

    public PlaybackControlBar()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PlayerBarViewModel>();
    }

    private void Slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        //ViewModel.EndSeeking();
    }
    private void Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        //ViewModel.StartSeeking();
    }
    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        //if (sender is Slider slider)
        //{
        //    ViewModel.SeekByMilliseconds(slider.Value);
        //}
    }

    private void VolumeSlider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
    }
}
