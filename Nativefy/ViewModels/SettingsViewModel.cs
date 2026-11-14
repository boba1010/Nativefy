using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nativefy.Services.LocalMetadata;
using Nativefy.Settings;

namespace Nativefy.ViewModels;

public sealed partial class SettingsViewModel(ISearchQueriesService search, IHistoryService history) : ObservableObject
{
    [ObservableProperty]
    public partial int MediaCacheSize { get; set; } = Config.MediaCacheSize / 1024;

    public int MaxMediaCacheSize => Config.MaxMediaCacheSize / 1024;
    public int MinMediaCacheSize => Config.MinMediaCacheSize / 1024;

    partial void OnMediaCacheSizeChanged(int value)
    {
        Config.MediaCacheSize = value * 1024;
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
    public void SaveCacheConfig()
    {
        Config.SaveConfig();
    }
}
