using Nativefy.JSON;
using Nativefy.Models.Media;
using System.Text.Json;
using Path = System.IO.Path;

namespace Nativefy.DbContexts;

public static class NativefyDatabaseProvider
{
    private static string _playlistsPath = null!;
    private static string _likedMediaPath = null!;
    private static string _historyPath = null!;
    private static string _searchQueriesPath = null!;

    public static List<LocalPlaylist> Playlists { get; private set; } = [];
    
    public static List<HistoryItem> History { get; private set; } = [];

    public static LikedMediaPlaylist LikedMedia { get; private set; } = new()
    {
        CreatedAt = DateTime.Now,
        Id = "LikedMedia",
        ImageUrl = "playlist.png",
        Name = "Liked Media",
        UpdatedAt = DateTime.Now,
    };

    public static List<string> SearchQueries { get; private set; } = [];

    // --------------- Saving ---------------
    public static void SaveChanges()
    {
        SaveSavedPlaylistsChanges();

        SaveLikedMediaChanges();

        SaveHistoryChanges();

        SaveSearchQueries();
    }

    public static void SaveSavedPlaylistsChanges()
    {
        using var fs = File.Create(_playlistsPath);

        JsonSerializer.Serialize(fs, Playlists, NativefyJsonContext.Default.ListLocalPlaylist);
    }

    public static void SaveLikedMediaChanges()
    {
        using var fs = File.Create(_likedMediaPath);

        JsonSerializer.Serialize(fs, LikedMedia, NativefyJsonContext.Default.ListMediaItem);
    }
    
    public static void SaveHistoryChanges()
    {
        using var fs = File.Create(_historyPath);

        JsonSerializer.Serialize(fs, History, NativefyJsonContext.Default.ListHistoryItem);
    }

    public static void SaveSearchQueries()
    {
        using var fs = File.Create(_searchQueriesPath);

        JsonSerializer.Serialize(fs, SearchQueries, NativefyJsonContext.Default.ListString);
    }

    // --------------- Loading ---------------
    public static void LoadDatabases()
    {
        LoadSavedPlaylists();

        LoadLikedMedia();

        LoadHistory();

        LoadSearchQueries();
    }

    private static void LoadSavedPlaylists()
    {
        using var fs = File.OpenRead(_playlistsPath);

        Playlists = JsonSerializer.Deserialize(fs, NativefyJsonContext.Default.ListLocalPlaylist) ?? [];
    }

    private static void LoadLikedMedia()
    {
        using var fs = File.OpenRead(_likedMediaPath);

        LikedMedia = JsonSerializer.Deserialize(fs, NativefyJsonContext.Default.LikedMediaPlaylist) ?? new()
        {
            CreatedAt = DateTime.Now,
            Id = "LikedMedia",
            ImageUrl = "playlist.png",
            Name = "Liked Media",
            UpdatedAt = DateTime.Now,
        };
    }

    private static void LoadHistory()
    {
        using var fs = File.OpenRead(_historyPath);

        History = JsonSerializer.Deserialize(fs, NativefyJsonContext.Default.ListHistoryItem) ?? [];
    }

    private static void LoadSearchQueries()
    {
        using var fs = File.OpenRead(_searchQueriesPath);

        SearchQueries = JsonSerializer.Deserialize(fs, NativefyJsonContext.Default.ListString) ?? [];
    }

    // ---------- Initialization ----------
    public static void InitializeDatabase()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appFolder = Path.Combine(localAppData, "Nativefy");
        Directory.CreateDirectory(appFolder);

        _playlistsPath = Path.Combine(appFolder, "nativefy.userplaylists");
        _likedMediaPath = Path.Combine(appFolder, "nativefy.userlikedmedia");
        _historyPath = Path.Combine(appFolder, "nativefy.userhistory");
        _searchQueriesPath = Path.Combine(appFolder, "nativefy.searchqueries");

        InitializeSavedPlaylists();

        InitializeLikedMedia();

        InitializeHistory();

        InitializeSearchQueries();
    }

    private static void InitializeSavedPlaylists()
    {
        if (!File.Exists(_playlistsPath) || new FileInfo(_playlistsPath).Length == 0)
        {
            using var fs = File.Create(_playlistsPath);

            JsonSerializer.Serialize(fs, Playlists, NativefyJsonContext.Default.ListLocalPlaylist);
        }
    }

    private static void InitializeLikedMedia()
    {
        if (!File.Exists(_likedMediaPath) || new FileInfo(_likedMediaPath).Length == 0)
        {
            using var fs = File.Create(_likedMediaPath);

            JsonSerializer.Serialize(fs, LikedMedia, NativefyJsonContext.Default.LikedMediaPlaylist);
        }
    }

    private static void InitializeHistory()
    {
        if (!File.Exists(_historyPath) || new FileInfo(_historyPath).Length == 0)
        {
            using var fs = File.Create(_historyPath);

            JsonSerializer.Serialize(fs, History, NativefyJsonContext.Default.ListHistoryItem);
        }
    }

    private static void InitializeSearchQueries()
    {
        if (!File.Exists(_searchQueriesPath) || new FileInfo(_searchQueriesPath).Length == 0)
        {
            using var fs = File.Create(_searchQueriesPath);

            JsonSerializer.Serialize(fs, SearchQueries, NativefyJsonContext.Default.ListString);
        }
    }
}