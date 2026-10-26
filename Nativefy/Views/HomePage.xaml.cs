namespace Nativefy.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; set; }
    public HomePage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<HomeViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await ViewModel.LoadHomeFeedAsync();

        base.OnNavigatedTo(e);
    }

    private void OnFilterRadioButtonClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnBannerSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void OnCardClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnPlayClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnBrowseAllFilterButtonClick(object sender, RoutedEventArgs e)
    {
    }
}
