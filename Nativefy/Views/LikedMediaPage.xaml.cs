using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Messages;
using Nativefy.Models.Media;
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

    private void LikedMediaPage_Loading(FrameworkElement sender, object args)
    {
        _ = ViewModel.LoadLikedMediaPlaylistAsync();
    }

    private void SharePlaylist_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("SHARE CLICKED");
    }

    private void UnlikeItem_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("UNLIKE CLICKED");
    }

    private void Items_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is MediaItem media)
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
                AddedDate = media.AddedDate,
            }));
        }
    }
}
