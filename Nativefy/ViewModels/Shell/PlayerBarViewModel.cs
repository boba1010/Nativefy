using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Nativefy.ViewModels.Shell;

public sealed partial class PlayerBarViewModel : BaseViewModel
{
    [ObservableProperty]
    public partial bool IsVisible { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlayPauseGlyph))]
    [NotifyPropertyChangedFor(nameof(PlayPauseTooltip))]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial string AlbumArtUrl { get; set; } = "https://picsum.photos/200";

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

    public string PlayPauseGlyph => IsPlaying ? "\uE769" : "\uE768";
    public string PlayPauseTooltip => IsPlaying ? "Pause" : "Play";

    public PlayerBarViewModel()
    {

    }

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
