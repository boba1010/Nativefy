namespace Nativefy.Views;

public sealed partial class LikedChannelsPage : Page
{
    public LikedChannelsViewModel ViewModel { get; }

    public LikedChannelsPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<LikedChannelsViewModel>();

        Loaded += LikedChannelsPage_Loaded;
    }

    private void LikedChannelsPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadChannels();
    }

    private void LikedChannel_Click(object sender, RoutedEventArgs e)
    {
        var tag = (string?)(sender as Button)?.Tag;
        if (tag is null)
            return;

        Frame.Navigate(typeof(ChannelPage), tag);
    }

    private void Unlike_Click(object sender, RoutedEventArgs e)
    {
        var tag = (string?)(sender as Button)?.Tag;
        if (tag is null)
            return;
        ViewModel.UnlikeChannel(tag);
    }
}
