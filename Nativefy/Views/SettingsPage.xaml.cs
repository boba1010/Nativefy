namespace Nativefy.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
    }

    private async void OnGitHubClick(object sender, RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/boba1010/Nativefy"));
    }
}
