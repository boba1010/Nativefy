using Nativefy.DbContexts;
using Nativefy.Engine.DI;
using Nativefy.Services;
using Nativefy.Services.Cache;
using Nativefy.Services.Feed;
using Nativefy.Services.LocalMetadata;
using Nativefy.Services.OnlineMetadata;
using Nativefy.Services.Playback;
using Nativefy.ViewModels.Shell;
using System.Runtime.InteropServices;

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

        NativefyDatabaseProvider.InitializeDatabase();
        NativefyDatabaseProvider.LoadDatabases();

        UnhandledException += Application_UnhandledException;
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IPlaylistsService, PlaylistsService>();
        services.AddSingleton<IOnlinePlaylistService, OnlinePlaylistService>();
        services.AddSingleton<ILikedMediaService, LikedMediaService>();
        services.AddSingleton<IStreamingBackend, StreamingBackend>();
        services.AddSingleton<IFeedService, HomeFeedService>();
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<ISearchQueriesService, SearchQueriesService>();
        services.AddSingleton<MediaLoader>();
        services.AddSingleton<MediaCache>();
        services.AddSingleton<PlaybackController>();
        services.AddNativefyEngine();

        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<PlayerBarViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PlaylistViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<LikedMediaViewModel>();
        services.AddTransient<OnlinePlaylistViewModel>();
        services.AddTransient<HistoryViewModel>();
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

    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBoxW(nint hWnd, string text, string caption, uint type);

    private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        MessageBoxW(0, e.Exception.ToString(), "Nativefy - Unexpected Error", 0x10 | 0x0);

        Current.Exit();
    }
}
