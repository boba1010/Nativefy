using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nativefy.Services.LocalMetadata;
using Nativefy.Settings;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;

namespace Nativefy.ViewModels;

public sealed partial class SettingsViewModel(ISearchQueriesService search, IHistoryService history) : ObservableObject
{
    [ObservableProperty]
    public partial int MediaCacheSize { get; set; } = Config.MediaCacheSize / 1024;

    [ObservableProperty]
    public partial bool DeleteDuplicateVideosWithDifferentResolutions { get; set; } = Config.DeleteDuplicateVideosWithDifferentResolutions;

    [ObservableProperty]
    public partial string SelectedBrowser { get; set; } = ParseBrowserToString(Config.Browser);

    public ObservableCollection<string> Browsers { get; } =
    [
        "None",
        "Microsoft Edge",
        "Google Chrome",
        "Mozilla Firefox",
        "Brave"
    ];

    public int MaxMediaCacheSize => Config.MaxMediaCacheSize / 1024;
    public int MinMediaCacheSize => Config.MinMediaCacheSize / 1024;

    private static string ParseBrowserToString(Browser browser)
    {
        return browser switch
        {
            Browser.None => "None",
            Browser.MicrosoftEdge => "Microsoft Edge",
            Browser.GoogleChrome => "Google Chrome",
            Browser.MozillaFirefox => "Mozilla Firefox",
            Browser.Brave => "Brave",
            _ => "None"
        };
    }

    private static Browser ParseStringToBrowser(string value)
    {
        return value switch
        {
            "Microsoft Edge" => Browser.MicrosoftEdge,
            "Google Chrome" => Browser.GoogleChrome,
            "Mozilla Firefox" => Browser.MozillaFirefox,
            "Brave" => Browser.Brave,
            _ => Browser.None
        };
    }

    partial void OnMediaCacheSizeChanged(int value)
    {
        Config.MediaCacheSize = value * 1024;
    }

    partial void OnSelectedBrowserChanged(string value)
    {
        Config.Browser = ParseStringToBrowser(value);
    }

    [RelayCommand]
    public void ClearSearch()
    {
        search.Clear();
    }

    [RelayCommand]
    public void ClearHistory()
    {
        history.ClearAll();
    }

    [RelayCommand]
    public void ClearSearchHistory()
    {
        history.ClearSearch();
    }

    [RelayCommand]
    public void ClearWatchedHistory()
    {
        history.ClearWatched();
    }

    [RelayCommand]
    public void SaveConfig()
    {
        Config.SaveConfig();
    }
}
