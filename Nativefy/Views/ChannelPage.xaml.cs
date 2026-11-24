using Nativefy.Engine.Channels;

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

    private void Play_Clicked(string id)
    {
        ViewModel.Play(id);
    }

    private void Description_Click(object sender, RoutedEventArgs e)
    {
        var description = (sender as Button)?.Tag;

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Content = description,
            Title = ViewModel.Title,
            PrimaryButtonText = "Close",
        };

        _ = dialog.ShowAsync();
    }

    private void Tab_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: ChannelTab tab })
            ViewModel.SelectedTab = tab;
    }
}
