using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Engine;
using Nativefy.Messages;
using System.Diagnostics;

namespace Nativefy.Views;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel { get; }

    public SearchPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<SearchViewModel>();

        WeakReferenceMessenger.Default.Register<SearchMessage>(this, (r, m) =>
        {
            QueryPreview.Text = m.Query;
            _ = ViewModel.ExecuteSearchAsync(m.Query);
        });
    }

    public static string FormatDuration(TimeSpan? duration)
    {
        if (!duration.HasValue)
            return "";
        return duration.Value.Hours >= 1 ? duration.Value.ToString(@"hh\:mm\:ss") : duration.Value.ToString(@"mm\:ss");
    } 

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string searchQuery)
        {
            QueryPreview.Text = searchQuery;
            _ = ViewModel.ExecuteSearchAsync(searchQuery);
        }
    }

    private void Result_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is IPlayable playable)
        {
            if (playable is IPlaylist playlist)
                Frame.Navigate(typeof(OnlinePlaylistPage), playlist.Id);
        }
    }

    private void Like_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;

        _ = ViewModel.LikeMediaAsync((string)btn.Tag);
    }

    private void Filter_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        Debug.WriteLine("FILTER CHANGED");
    }
}
