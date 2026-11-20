namespace Nativefy.Views;

public sealed partial class ChannelPage : Page
{
    public ChannelViewModel ViewModel { get; }

    public ChannelPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<ChannelViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string id)
            _ = ViewModel.LoadChannelAsync(id);
    }

    private void Play_Clicked(object sender, RoutedEventArgs e)
    {
        var tag = (sender as Button)?.Tag;
        if (tag is null)
            return;

        ViewModel.Play((string)tag);
    }
}
