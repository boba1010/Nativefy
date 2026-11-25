using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Playback;
using Nativefy.Services.LocalMetadata;
using Nativefy.Services.Playback;
using System.Threading.Tasks;

namespace Nativefy.ViewModels.Shell;

public enum DownloadStatus
{
    Downloading,

    Downloaded,

    Failed,

    Canceled,
}

public sealed partial class PlayerBarViewModel : BaseViewModel
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly IHistoryService _history;

    private Notification _downloadedNotification = new() { Severity = Severity.Info };
    private Notification _downloadProgressNotification = new() { Severity = Severity.Info };
    private Notification _downloadFailedNotification = new() { Severity = Severity.Failure };
    private Notification _downloadCanceledNotification = new() { Severity = Severity.Warning };

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

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    public string DownloadSpeed { get; set; } = string.Empty;

    public string DownloadEta { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DownloadStatus DownloadStatus { get; set; }

    public PlayerBarViewModel(PlaybackController controller, IHistoryService history)
    {
        PlaybackController = controller;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _history = history;

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
            Show();

            _history.UpdateOrAdd(new HistoryPlaylist()
            {
                Id = playlist.Id,
                Title = playlist.Title,
                Author = playlist.Author,
                ImageUrl = playlist.ImageUrl,
                Url = playlist.Url,
                Items = [.. playlist.Items.Select(i => new HistoryMediaItem(i.Id, i.Url, i.ImageUrl, i.Title, i.Author, i.Duration, i.Position))]
            });

            await PlaybackController.PlayPlaylistAsync(playlist);
        });

        WeakReferenceMessenger.Default.Register<PlayItemMessage>(this, async (r, m) => 
        {
            var item = m.Item;

            PlaybackController.ClearQueue();

            ImageUrl = item.ImageUrl;
            ItemTitle = item.Title;
            ChannelName = item.Author;
            item.Duration = item.Duration.Subtract(new(0, 0, 1));
            Duration = item.Duration.TotalMilliseconds;
            Position = 0;
            Show();

            _history.UpdateOrAdd(new HistoryMediaItem(item.Id, item.Url, item.ImageUrl, item.Title, item.Author, item.Duration, -1));

            await PlaybackController.PlayItemAsync(item);
        });

        WeakReferenceMessenger.Default.Register<MediaReportMessage>(
            this,
            (r, m) =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    switch (m.Stage)
                    {
                        case MediaReportStage.Downloading:
                            DownloadStatus = DownloadStatus.Downloading;
                            DownloadProgress = 0;

                            UpdateNotifications();

                            _downloadProgressNotification.Title = $"Downloading '{ItemTitle}'";
                            WeakReferenceMessenger.Default.Send(new NotificationMessage(_downloadProgressNotification));
                            break;

                        case MediaReportStage.DownloadProgress:
                            if (m.Progress is not { } progress)
                                break;

                            DownloadProgress = progress.Percentage;
                            DownloadSpeed = progress.BytesPerSecond is { } speed ? $"{speed / 1024 / 1024:F2} MB/s" : string.Empty;
                            DownloadEta = progress.EstimatedRemaining is { } eta ? eta.ToString(@"mm\:ss") : string.Empty;

                            var message = $"Progress: {progress.Percentage:F1}%\n";
                            message += $"Speed: {DownloadSpeed}\n";
                            message += $"ETA: {DownloadEta}";

                            _downloadProgressNotification.Message = message;
                            WeakReferenceMessenger.Default.Send(new NotificationMessage(_downloadProgressNotification));
                            break;

                        case MediaReportStage.Downloaded:
                            DownloadStatus = DownloadStatus.Downloaded;
                            DownloadProgress = 100;
                            DownloadSpeed = string.Empty;
                            DownloadEta = string.Empty;

                            _downloadedNotification.Title = $"Downloaded '{ItemTitle}' Successfully";
                            WeakReferenceMessenger.Default.Send(new NotificationMessage(_downloadedNotification));
                            break;

                        case MediaReportStage.Loaded:
                            DownloadStatus = DownloadStatus.Downloaded;
                            break;

                        case MediaReportStage.Cancelled:
                            DownloadStatus = DownloadStatus.Canceled;
                            DownloadProgress = 100;
                            DownloadSpeed = string.Empty;
                            DownloadEta = string.Empty;

                            _downloadCanceledNotification.Title = $"Download '{ItemTitle}' Canceled";
                            WeakReferenceMessenger.Default.Send(new NotificationMessage(_downloadCanceledNotification));
                            break;
                        case MediaReportStage.Failed:
                            DownloadStatus = DownloadStatus.Failed;
                            DownloadProgress = 100;
                            DownloadSpeed = string.Empty;
                            DownloadEta = string.Empty;

                            _downloadFailedNotification.Title = $"Download '{ItemTitle}' Failed";
                            WeakReferenceMessenger.Default.Send(new NotificationMessage(_downloadFailedNotification));
                            break;
                    }
                });
            });

        PlaybackController.CurrentItemChanged += item =>
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                ImageUrl = item.ImageUrl;
                ItemTitle = item.Title;
                ChannelName = item.Author;
                Duration = item.Duration.Add(new(0, 0, 1)).TotalMilliseconds;
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

    private void UpdateNotifications()
    {
        _downloadedNotification = new() { Severity = Severity.Success };
        _downloadProgressNotification = new() { Severity = Severity.Info };
        _downloadFailedNotification = new() { Severity = Severity.Failure };
        _downloadCanceledNotification = new() { Severity = Severity.Warning };
    }

    private static void Show()
    {
        WeakReferenceMessenger.Default.Send<PlayerBarVisiblityChangedMessage>(new(true));
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
    public void SkipBackward()
    {
        DownloadStatus = DownloadStatus.Downloaded;
        PlaybackController.SkipPrevious();
    }

    [RelayCommand]
    public void PlayPause() => PlaybackController.TogglePlayPause();

    [RelayCommand]
    public void SkipForward()
    {
        DownloadStatus = DownloadStatus.Downloaded;
        PlaybackController.SkipNext();
    }

    public void ToggleShuffle() => PlaybackController.ToggleShuffle();

    [RelayCommand]
    public async Task ToggleDetailsPanelAsync()
    {
    }

    [RelayCommand]
    public void CancelDownload() => PlaybackController.CancelDownload();

    public void SetVolume(double value) => PlaybackController.SetVolume(value);

    private void UpdateQueueState()
    {
        QueueCount = PlaybackController.QueueCount;
        CurrentIndex = PlaybackController.CurrentIndex;
    }
}
