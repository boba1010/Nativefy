using Nativefy.Models.Media;
using System.Diagnostics;

namespace Nativefy.Views;

public sealed partial class OnlinePlaylistPage : Page
{
    public OnlinePlaylistViewModel ViewModel { get; }

    public OnlinePlaylistPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<OnlinePlaylistViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string id)
            _ = ViewModel.LoadPlaylistAsync(id);
    }

    private void Items_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is MediaItem item)
            ViewModel.PlayItem(item);
    }

    private void SharePlaylist_Click(object sender, RoutedEventArgs e)
    {
    }

    private void LikeItem_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        var url = (string)btn.Tag;
        Debug.WriteLine($"LIKED ITEM: {url}");
        _ = ViewModel.LikeItemAsync(url);
    }
}
