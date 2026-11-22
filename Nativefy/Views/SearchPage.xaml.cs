using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Engine.Search;
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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string searchQuery)
        {
            QueryPreview.Text = searchQuery;
            _ = ViewModel.ExecuteSearchAsync(searchQuery);
        }
    }

    private void Result_ItemInvoked(object sender, RoutedEventArgs e)
    {
        var result = (sender as Button)?.Tag as ISearchResult;

        if (result is IPlayableSearchResult playable)
        {
            if (playable is Playlist playlist)
                Frame.Navigate(typeof(OnlinePlaylistPage), playlist.Id);
            else if (playable is Video video)
            {
                WeakReferenceMessenger.Default.Send(new PlayItemMessage(new()
                {
                    Id = video.Id,
                    Url = video.Url,
                    Author = video.Author,
                    Duration = video.Duration,
                    ImageUrl = video.ImageUrl,
                    Title = video.Title,
                    Position = 1,
                }));
            }
        }
        else if (result is Channel channel)
            Frame.Navigate(typeof(ChannelPage), channel.Id);
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
