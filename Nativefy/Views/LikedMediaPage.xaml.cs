using CommunityToolkit.Mvvm.ComponentModel;
using Nativefy.Models.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Nativefy.Views;

public sealed partial class LikedMediaPage : Page
{
    public LikedMediaViewModel ViewModel { get; }
    public LikedMediaPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<LikedMediaViewModel>();

        Loading += LikedMediaPage_Loading;
    }

    private async void LikedMediaPage_Loading(FrameworkElement sender, object args)
    {
        await ViewModel.LoadLikedMediaPlaylistAsync();
    }

    private async void SharePlaylist_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("SHARE CLICKED");
    }

    private async void UnlikeItem_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("UNLIKE CLICKED");
    }

    private void Items_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        Debug.WriteLine("ITEM INVOKED");
    }
}
