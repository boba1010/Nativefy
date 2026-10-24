using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Nativefy.ViewModels.Shell;

public sealed partial class PlayerBarViewModel : BaseViewModel
{
    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial string AlbumArtUrl { get; set; } = null!;

    [ObservableProperty]
    public partial string TrackTitle { get; set; } = null!;

    [ObservableProperty]
    public partial string ArtistName { get; set; } = null!;

    [ObservableProperty]
    public partial string PositionText { get; set; } = null!;

    [ObservableProperty]
    public partial string DurationText { get; set; } = null!;

    [ObservableProperty]
    public partial double Position { get; set; }

    [ObservableProperty]
    public partial double SliderMaximum { get; set; }

    [RelayCommand]
    public async Task ToggleShuffleAsync()
    {
    }

    [RelayCommand]
    public async Task SkipBackwardAsync()
    {
    }

    [RelayCommand]
    public async Task PlayPauseAsync()
    {
    }

    [RelayCommand]
    public async Task SkipForwardAsync()
    {
    }

    [RelayCommand]
    public async Task ToggleRepeatAsync()
    {
    }

    [RelayCommand]
    public async Task ToggleQueuePanelAsync()
    {
    }

    [RelayCommand]
    public async Task ToggleDetailsPanelAsync()
    {
    }

    [RelayCommand]
    public async Task ToggleLyricsPanelAsync()
    {
    }
}
