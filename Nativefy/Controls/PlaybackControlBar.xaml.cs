using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using Nativefy.Messages;
using Nativefy.ViewModels.Shell;
using Nativefy.Views;

namespace Nativefy.Controls;

public enum RepeatState
{
    Off,
    RepeatAll,
    RepeatOne
}

public sealed partial class PlaybackControlBar : UserControl
{
    public PlayerBarViewModel ViewModel { get; set; }

    public PlaybackControlBar()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PlayerBarViewModel>();

        MediaSlider.AddHandler(UIElement.PointerPressedEvent,new PointerEventHandler(Slider_PointerPressed), handledEventsToo: true);
    }

    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            if (slider.IntermediateValue == e.NewValue)
                ViewModel.IsUserScrubbing = true;
        }
    }

    private void Slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Slider slider)
            ViewModel.SeekTo(slider.Value);
    }

    private void Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.IsUserScrubbing = true;
    }

    private void Volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
            ViewModel?.SetVolume(slider.Value);
    }

    private void RepeatButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleRepeat();

        UpdateRepeatUIVisuals();
    }

    private void UpdateRepeatUIVisuals()
    {
        switch (ViewModel.PlaybackController.RepeatState)
        {
            case RepeatState.Off:
                RepeatIcon.Icon = Icon.ArrowRepeatAllOff;
                ToolTipService.SetToolTip(RepeatButton, "Repeat Off");
                break;

            case RepeatState.RepeatAll:
                RepeatIcon.Icon = Icon.ArrowRepeatAll;
                ToolTipService.SetToolTip(RepeatButton, "Repeat All");
                break;

            case RepeatState.RepeatOne:
                RepeatIcon.Icon = Icon.ArrowRepeat1;
                ToolTipService.SetToolTip(RepeatButton, "Repeat One");
                break;
        }
    }
    
    private void Shuffle_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleShuffle();
        UpdateShuffleUIVisuals();
    }

    private void UpdateShuffleUIVisuals()
    {
        if (ViewModel.PlaybackController.IsShuffleOn)
        {
            ShuffleIcon.Icon = Icon.ArrowShuffle;
            ToolTipService.SetToolTip(ShuffleButton, "Shuffle On");
        }
        else
        {
            ShuffleIcon.Icon = Icon.ArrowShuffleOff;
            ToolTipService.SetToolTip(ShuffleButton, "Shuffle Off");
        }
    }

    private void GoToMediaPlayerPage_Click(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(MediaPlayerPage)));
    }
}
