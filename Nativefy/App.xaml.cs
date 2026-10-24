using Nativefy.ViewModels.Shell;
using System.Diagnostics;

namespace Nativefy;

public partial class App : Application
{
    private Window? _window;

    public static IServiceProvider Services { get; set; } = null!;

    public App()
    {
        InitializeComponent();

        var services = new ServiceCollection();

        ConfigureServices(services);

        Services = services.BuildServiceProvider();

        UnhandledException += Application_UnhandledException;
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddTransient<HomeViewModel>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<PlayerBarViewModel>();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        try
        {
            string message = e.Message;

            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "crash-report.json");
            File.WriteAllText(tempPath, message);

            Process.Start("D:\\My Programs\\Nativefy\\Nativefy\\bin\\CrashReporter.exe");
        }
        catch (Exception)
        {
            Current.Exit();
        }
    }
}
