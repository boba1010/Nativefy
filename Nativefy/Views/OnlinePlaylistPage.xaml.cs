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

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string id)
            await ViewModel.LoadPlaylistAsync(id);
    }

    private void Items_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {

    }

    private void SharePlaylist_Click(object sender, RoutedEventArgs e)
    {
    }

    private async void LikeItem_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        var url = (string)btn.Tag;
        Debug.WriteLine($"LIKED ITEM: {url}");
        await ViewModel.LikeItemAsync(url);
    }
}
