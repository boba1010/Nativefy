using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
using System.Diagnostics;

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

    private void History_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is HistoryItem historyItem)
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

    private void Like_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;

        ViewModel.LikeMedia((string)btn.Tag);
    }
}
