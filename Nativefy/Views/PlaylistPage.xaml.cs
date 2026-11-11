using Nativefy.Models.Media;
using System.Diagnostics;

namespace Nativefy.Views;

public sealed partial class PlaylistPage : Page
{
    public PlaylistViewModel ViewModel { get; }

    public PlaylistPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PlaylistViewModel>();
    }

    private static readonly SolidColorBrush StripeBrushOdd = new(Windows.UI.Color.FromArgb(0x0A, 0xFF, 0xFF, 0xFF));
    private static readonly SolidColorBrush StripeBrushEven = new(Microsoft.UI.Colors.Transparent);
    public static Brush StripeBrush(int position) => position % 2 == 0 ? StripeBrushOdd : StripeBrushEven;
    public static string FormatDate(DateTimeOffset d) => d.ToString("MMM d");
    
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string id)
            ViewModel.LoadPlaylist(id);
    }

    private void LikeItem_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        ViewModel.LikeTrack((string)btn.Tag);
    }

    private void Items_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is MediaItem item)
            ViewModel.PlayItem(item);
    }

    private void SharePlaylist_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("SHARE CLICKED!");
    }
}
