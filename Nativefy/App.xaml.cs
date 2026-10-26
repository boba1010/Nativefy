using Microsoft.EntityFrameworkCore;
using Nativefy.DbContexts;
using Nativefy.Engine;
using Nativefy.Engine.DI;
using Nativefy.Services;
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

    private static void ConfigureServices(ServiceCollection services)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appFolder = System.IO.Path.Combine(localAppData, "Nativefy");
        System.IO.Directory.CreateDirectory(appFolder);
        string dbPath = System.IO.Path.Combine(appFolder, "nativefy.db");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        services.AddSingleton<IPlaylistsService, PlaylistsService>();
        services.AddNativefyEngine();

        services.AddTransient<HomeViewModel>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<PlayerBarViewModel>();
        services.AddSingleton<PlaylistViewModel>();
        services.AddSingleton<SearchViewModel>();
    }


    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

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
