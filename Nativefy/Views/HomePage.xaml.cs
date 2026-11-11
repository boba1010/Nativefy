namespace Nativefy.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; set; }

    public HomePage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<HomeViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _ = ViewModel.LoadHomeFeedAsync();
    }

    private void OnFilterRadioButtonClick(object sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void Play_Clicked(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        ViewModel.Play((string)button.Tag);
    }
}
