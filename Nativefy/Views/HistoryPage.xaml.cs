using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;

namespace Nativefy.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryViewModel ViewModel { get; }

    public HistoryPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<HistoryViewModel>();

        Loaded += HistoryPage_Loaded;
    }

    private void HistoryPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadHistory();
    }

    private void OnRadioFilterButtonChecked(object sender, RoutedEventArgs e)
    {
        var btn = (RadioButton)sender;
        ViewModel?.Filter((string)btn.Tag);
    }

    private void Like_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;

        ViewModel.LikeMedia((string)btn.Tag);
    }

    private void ItemContainer_Tapped(object sender, TappedRoutedEventArgs e)
    {
        DependencyObject? source = e.OriginalSource as DependencyObject;
        while (source != null && !ReferenceEquals(source, sender))
        {
            if (source is ButtonBase)
                return;
            source = VisualTreeHelper.GetParent(source);
        }

        var tag = (sender as ItemContainer)?.Tag;

        if (tag is HistoryItem historyItem)
        {
            if (historyItem is HistoryPlaylist playlist)
                Frame.Navigate(typeof(PlaylistPage), playlist.Id);
            else if (historyItem is HistoryMediaItem media)
            {
                WeakReferenceMessenger.Default.Send(new PlayItemMessage(new()
                {
                    Id = media.Id,
                    Url = media.Url,
                    Author = media.Author,
                    Duration = media.Duration,
                    ImageUrl = media.ImageUrl,
                    Title = media.Title,
                    Position = media.Position,
                }));
            }
        }
    }
}
