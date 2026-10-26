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
    public static string FormatDuration(TimeSpan t) => t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");

    public static string FormatTotalDuration(List<PlaylistTrack> tracks)
    {
        var hours = 0;
        var minutes = 0;

        foreach (var track in tracks)
        {
            hours += track.Track.Duration.Hours;
            minutes += track.Track.Duration.Minutes;
        }

        if (hours >= 1)
            return $"{hours} hr {minutes} min";

        return $"{minutes} min";
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string id)
            await ViewModel.LoadPlaylistAsync(id);

        base.OnNavigatedTo(e);
    }

    private async void LikeTrack_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("LIKE CLICKED!");

        var btn = (Button)sender;
        await ViewModel.LikeTrackAsync((string)btn.Tag);
    }

    private void Tracks_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is PlaylistTrack track)
        {
            Debug.WriteLine("TRACK CLICKED!");
        }
    }

    private void SharePlaylist_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("SHARE CLICKED!");
    }
}
