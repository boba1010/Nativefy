using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Nativefy.Messages;
using Nativefy.Services;
using System.Threading.Tasks;

namespace Nativefy.ViewModels.Shell;

public sealed partial class PlayerBarViewModel : BaseViewModel
{
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    public partial bool IsVisible { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlayPauseGlyph))]
    [NotifyPropertyChangedFor(nameof(PlayPauseTooltip))]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial string ImageUrl { get; set; } = "https://picsum.photos/1280/720";

    [ObservableProperty]
    public partial string ItemTitle { get; set; } = null!;

    [ObservableProperty]
    public partial string ChannelName { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PositionText))]
    public partial double Position { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    public partial double Duration { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsForwardEnabled))]
    [NotifyPropertyChangedFor(nameof(IsBackwardEnabled))]
    public partial uint CurrentIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsForwardEnabled))]
    public partial uint QueueCount { get; set; }

    public bool IsForwardEnabled => QueueCount > 0 && CurrentIndex < QueueCount - 1;
    public bool IsBackwardEnabled => QueueCount > 0 && CurrentIndex > 0;
    public string PositionText => TimeSpan.FromMilliseconds(Position).ToString(@"mm\:ss");
    public string DurationText => TimeSpan.FromMilliseconds(Duration).ToString(@"mm\:ss");
    public string PlayPauseGlyph => IsPlaying ? "\uE769" : "\uE768";
    public string PlayPauseTooltip => IsPlaying ? "Pause" : "Play";

    public PlaybackController PlaybackController { get; }
    public bool IsUserScrubbing { get; set; }
    
    public PlayerBarViewModel(PlaybackController controller)
    {
        PlaybackController = controller;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        WeakReferenceMessenger.Default.Register<PlayPlaylistMessage>(this, async (r, m) => 
        {
            var playlist = m.Playlist;

            if (playlist.Items.Count <= 0)
                return;

            PlaybackController.ClearQueue();

            var firstItem = playlist.Items[0];
            ImageUrl = firstItem.ImageUrl;
            ItemTitle = firstItem.Title;
            ChannelName = firstItem.Author;
            Duration = firstItem.Duration.TotalMilliseconds;
            Position = 0;
            IsVisible = true;

            await PlaybackController.PlayPlaylistAsync(playlist);
        });

        WeakReferenceMessenger.Default.Register<PlayItemMessage>(this, async (r, m) => 
        {
            var item = m.Item;

            PlaybackController.ClearQueue();

            ImageUrl = item.ImageUrl;
            ItemTitle = item.Title;
            ChannelName = item.Author;
            Duration = item.Duration.TotalMilliseconds;
            Position = 0;
            IsVisible = true;

            await PlaybackController.PlayItemAsync(item);
        });

        PlaybackController.CurrentItemChanged += item =>
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                ImageUrl = item.ImageUrl;
                ItemTitle = item.Title;
                ChannelName = item.Author;
                Duration = item.Duration.TotalMilliseconds;
                Position = 0;
            });

            _dispatcherQueue.TryEnqueue(UpdateQueueState);
        };

        PlaybackController.QueueChanged += () =>
        {
            _dispatcherQueue.TryEnqueue(UpdateQueueState);
        };

        PlaybackController.StateChanged += Player_StateChanged;
        PlaybackController.PositionChanged += Player_PositionChanged;
        PlaybackController.SeekCompleted += Player_SeekCompleted;
    }

    private void Player_SeekCompleted() => IsUserScrubbing = false;
    
    private void Player_PositionChanged(TimeSpan position)
    {
        if (IsUserScrubbing) return;
        _dispatcherQueue.TryEnqueue(() => Position = position.TotalMilliseconds);
    }

    private void Player_StateChanged(PlayerState state)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            if (state is PlayerState.Paused)
                IsPlaying = false;
            else
                IsPlaying = true;
        });
    }

    public void SeekTo(double milliseconds)
    {
        PlaybackController.Seek(TimeSpan.FromMilliseconds(milliseconds));
    }

    public void ToggleRepeat() => PlaybackController.ToggleRepeat();

    [RelayCommand]
    public void SkipBackward() => PlaybackController.SkipPrevious();

    [RelayCommand]
    public void PlayPause() => PlaybackController.TogglePlayPause();

    [RelayCommand]
    public void SkipForward() => PlaybackController.SkipNext();

    public void ToggleShuffle() => PlaybackController.ToggleShuffle();

    [RelayCommand]
    public async Task ToggleDetailsPanelAsync()
    {
    }

    public void SetVolume(double value) => PlaybackController.SetVolume(value);

    private void UpdateQueueState()
    {
        QueueCount = PlaybackController.QueueCount;
        CurrentIndex = PlaybackController.CurrentIndex;
    }
}
