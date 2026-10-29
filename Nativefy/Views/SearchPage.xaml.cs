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

        WeakReferenceMessenger.Default.Register<SearchMessage>(this, async (r, m) =>
        {
            QueryPreview.Text = m.Query;
            await ViewModel.ExecuteSearchAsync(m.Query);
        });
    }

    public static string FormatDuration(TimeSpan? duration)
    {
        if (!duration.HasValue)
            return "";
        return duration.Value.Hours >= 1 ? duration.Value.ToString(@"hh\:mm\:ss") : duration.Value.ToString(@"mm\:ss");
    } 

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string searchQuery)
        {
            QueryPreview.Text = searchQuery;
            await ViewModel.ExecuteSearchAsync(searchQuery);
        }

        base.OnNavigatedTo(e);
    }

    private void Result_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        Debug.WriteLine("RESULT INVOKED");

        if (args.InvokedItem is IPlayable playable)
        {
            if (playable is IPlaylist playlist)
                Frame.Navigate(typeof(OnlinePlaylistPage), playlist.Id);
        }
    }

    private async void Like_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("LIKE CLICK");

        var btn = (Button)sender;

        await ViewModel.LikeMediaAsync((string)btn.Tag);
    }

    private void Filter_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        Debug.WriteLine("FILTER CHANGED");
    }
}
