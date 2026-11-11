using Nativefy.Models.Media;
using Path = System.IO.Path;

namespace Nativefy.DbContexts;

public static class NativefyDatabaseProvider
{
    private const string Header = "NATIVEFYAPPDB";
    private const string PlaylistsHeader = "SAVEDPLAYLISTS";
    private const string LikedMediaHeader = "LIKEDMEDIA";
    private const string HistoryHeader = "HISTORY";
    private const string SearchQueriesHeader = "SEARCHQUERIES";
    private const int SchemaVersion = 1;

    private static string _playlistsPath = null!;
    private static string _likedMediaPath = null!;
    private static string _historyPath = null!;
    private static string _searchQueriesPath = null!;

    public static List<LocalPlaylist> Playlists { get; private set; } = [];
    
    public static List<MediaItem> History { get; private set; } = [];

    public static LikedMediaPlaylist LikedMedia { get; private set; } = new()
    {
        CreatedAt = DateTime.Now,
        Id = LikedMediaHeader,
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
    }

    public static void SaveSavedPlaylistsChanges()
    {
        using var fs = File.Create(_playlistsPath);
        using var writer = new BinaryWriter(fs);

        writer.Write(Header);

        writer.Write(PlaylistsHeader);

        writer.Write(SchemaVersion);

        writer.Write(Playlists.Count);

        foreach (var playlist in Playlists)
        {
            writer.Write(playlist.Id);
            writer.Write(playlist.Name);
            writer.Write(playlist.ImageUrl);
            writer.Write(playlist.CreatedAt.ToBinary());
            writer.Write(playlist.UpdatedAt.ToBinary());

            writer.Write(playlist.Items.Count);

            foreach (var item in playlist.Items)
            {
                writer.Write(item.Id);
                writer.Write(item.Title);
                writer.Write(item.Author);
                writer.Write(item.ImageUrl);
                writer.Write(item.Url);
                writer.Write(item.Position);
                writer.Write(item.AddedDate.ToBinary());
                writer.Write(item.Duration.Ticks);
            }
        }
    }

    public static void SaveLikedMediaChanges()
    {
        using var fs = File.Create(_likedMediaPath);
        using var writer = new BinaryWriter(fs);

        writer.Write(Header);

        writer.Write(LikedMediaHeader);

        writer.Write(SchemaVersion);

        writer.Write(LikedMedia.Items.Count);

        writer.Write(LikedMedia.Id);
        writer.Write(LikedMedia.Name);
        writer.Write(LikedMedia.ImageUrl);
        writer.Write(LikedMedia.CreatedAt.ToBinary());
        writer.Write(LikedMedia.UpdatedAt.ToBinary());

        foreach (var item in LikedMedia.Items)
        {
            writer.Write(item.Id);
            writer.Write(item.Title);
            writer.Write(item.Author);
            writer.Write(item.ImageUrl);
            writer.Write(item.Url);
            writer.Write(item.Position);
            writer.Write(item.AddedDate.ToBinary());
            writer.Write(item.Duration.Ticks);
        }
    }
    
    public static void SaveHistoryChanges()
    {
        using var fs = File.Create(_historyPath);
        using var writer = new BinaryWriter(fs);

        writer.Write(Header);

        writer.Write(HistoryHeader);

        writer.Write(SchemaVersion);

        writer.Write(History.Count);

        foreach (var item in History)
        {
            writer.Write(item.Id);
            writer.Write(item.Title);
            writer.Write(item.Author);
            writer.Write(item.ImageUrl);
            writer.Write(item.Url);
            writer.Write(item.Position);
            writer.Write(item.AddedDate.ToBinary());
            writer.Write(item.Duration.Ticks);
        }
    }

    public static void SaveSearchQueries()
    {
        using var fs = File.Create(_searchQueriesPath);
        using var writer = new BinaryWriter(fs);

        writer.Write(Header);

        writer.Write(SearchQueriesHeader);

        writer.Write(SchemaVersion);

        writer.Write(SearchQueries.Count);

        foreach (var item in SearchQueries)
            writer.Write(item);
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
        try
        {
            using var fs = File.OpenRead(_playlistsPath);
            using var reader = new BinaryReader(fs);
            var header = reader.ReadString();
            if (header != Header)
                throw new InvalidOperationException("INVALID HEADER IN PLAYLISTS");

            var tableSig = reader.ReadString();

            var schemaVersion = reader.ReadInt32();
            if (schemaVersion != SchemaVersion)
                throw new InvalidOperationException("INVALID SCHEMA VERSION IN PLAYLISTS");

            var count = reader.ReadInt32();
            var playlists = new List<LocalPlaylist>(count);

            for (int i = 0; i < count; i++)
            {
                var playlist = new LocalPlaylist
                {
                    Id = reader.ReadString(),
                    Name = reader.ReadString(),
                    ImageUrl = reader.ReadString(),
                    CreatedAt = DateTime.FromBinary(reader.ReadInt64()),
                    UpdatedAt = DateTime.FromBinary(reader.ReadInt64()),
                };

                int itemCount = reader.ReadInt32();
                playlist.Items = new List<MediaItem>(itemCount);

                for (int j = 0; j < itemCount; j++)
                {
                    playlist.Items.Add(new MediaItem
                    {
                        Id = reader.ReadString(),
                        Title = reader.ReadString(),
                        Author = reader.ReadString(),
                        ImageUrl = reader.ReadString(),
                        Url = reader.ReadString(),
                        Position = reader.ReadInt32(),
                        AddedDate = DateTime.FromBinary(reader.ReadInt64()),
                        Duration = TimeSpan.FromTicks(reader.ReadInt64()),
                    });
                }

                playlists.Add(playlist);
            }

            Playlists = playlists;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Playlists Load Fail (Using Empty Fallback): {ex.Message}");
            Playlists = [];
        }
    }

    private static void LoadLikedMedia()
    {
        try
        {
            using var fs = File.OpenRead(_likedMediaPath);
            using var reader = new BinaryReader(fs);
            var header = reader.ReadString();
            if (header != Header)
                throw new InvalidOperationException("INVALID HEADER IN LIKED MEDIA");

            var tableSig = reader.ReadString();

            var schemaVersion = reader.ReadInt32();
            if (schemaVersion != SchemaVersion)
                throw new InvalidOperationException("INVALID SCHEMA VERSION IN LIKED MEDIA");

            int itemCount = reader.ReadInt32();

            if (fs.Position < fs.Length)
            {
                var likedMediaPlaylist = new LikedMediaPlaylist
                {
                    Id = reader.ReadString(),
                    Name = reader.ReadString(),
                    ImageUrl = reader.ReadString(),
                    CreatedAt = DateTime.FromBinary(reader.ReadInt64()),
                    UpdatedAt = DateTime.FromBinary(reader.ReadInt64()),
                    Items = new List<MediaItem>(itemCount)
                };

                for (int j = 0; j < itemCount; j++)
                {
                    likedMediaPlaylist.Items.Add(new MediaItem
                    {
                        Id = reader.ReadString(),
                        Title = reader.ReadString(),
                        Author = reader.ReadString(),
                        ImageUrl = reader.ReadString(),
                        Url = reader.ReadString(),
                        Position = reader.ReadInt32(),
                        AddedDate = DateTime.FromBinary(reader.ReadInt64()),
                        Duration = TimeSpan.FromTicks(reader.ReadInt64()),
                    });
                }

                LikedMedia = likedMediaPlaylist;
            }
            else
            {
                LikedMedia = new LikedMediaPlaylist { Id = "LIKEDMEDIA", Name = "Liked Media", Items = [] };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Liked Media Load Fail (Using Empty Fallback): {ex.Message}");
            LikedMedia = new LikedMediaPlaylist { Id = "LIKEDMEDIA", Items = [] };
        }
    }

    private static void LoadHistory()
    {
        try
        {
            using var fs = File.OpenRead(_historyPath);
            using var reader = new BinaryReader(fs);
            var header = reader.ReadString();
            if (header != Header)
                throw new InvalidOperationException("INVALID HEADER IN HISTORY");

            var tableSig = reader.ReadString();

            var schemaVersion = reader.ReadInt32();
            if (schemaVersion != SchemaVersion)
                throw new InvalidOperationException("INVALID SCHEMA VERSION IN HISTORY");

            int itemCount = reader.ReadInt32();

            if (fs.Position < fs.Length)
            {
                var history = new List<MediaItem>(itemCount);

                for (int j = 0; j < itemCount; j++)
                {
                    history.Add(new MediaItem
                    {
                        Id = reader.ReadString(),
                        Title = reader.ReadString(),
                        Author = reader.ReadString(),
                        ImageUrl = reader.ReadString(),
                        Url = reader.ReadString(),
                        Position = reader.ReadInt32(),
                        AddedDate = DateTime.FromBinary(reader.ReadInt64()),
                        Duration = TimeSpan.FromTicks(reader.ReadInt64()),
                    });
                }

                History = history;
            }
            else
            {
                History = [];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"History Load Fail (Using Empty Fallback): {ex.Message}");
            History = [];
        }
    }

    private static void LoadSearchQueries()
    {
        try
        {
            using var fs = File.OpenRead(_searchQueriesPath);
            using var reader = new BinaryReader(fs);
            var header = reader.ReadString();
            if (header != Header)
                throw new InvalidOperationException("INVALID HEADER IN SEARCHQUERIES");

            var tableSig = reader.ReadString();

            var schemaVersion = reader.ReadInt32();
            if (schemaVersion != SchemaVersion)
                throw new InvalidOperationException("INVALID SCHEMA VERSION IN SEARCHQUERIES");

            int count = reader.ReadInt32();

            if (fs.Position < fs.Length)
            {
                var queries = new List<string>(count);

                for (int i = 0; i < count; i++)
                    queries.Add(reader.ReadString());

                SearchQueries = queries;
            }
            else
            {
                SearchQueries = [];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search Queries Load Fail (Using Empty Fallback): {ex.Message}");
            SearchQueries = [];
        }
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
            using var binaryWriter = new BinaryWriter(fs);
            binaryWriter.Write(Header);
            binaryWriter.Write(PlaylistsHeader);
            binaryWriter.Write(SchemaVersion);
            binaryWriter.Write(0);
        }
    }

    private static void InitializeLikedMedia()
    {
        if (!File.Exists(_likedMediaPath) || new FileInfo(_likedMediaPath).Length == 0)
        {
            using var fs = File.Create(_likedMediaPath);
            using var writer = new BinaryWriter(fs);
            writer.Write(Header);
            writer.Write(LikedMediaHeader);
            writer.Write(SchemaVersion);
            writer.Write(0);
            writer.Write(LikedMedia.Id);
            writer.Write(LikedMedia.Name);
            writer.Write(LikedMedia.ImageUrl);
            writer.Write(LikedMedia.CreatedAt.ToBinary());
            writer.Write(LikedMedia.UpdatedAt.ToBinary());
        }
    }

    private static void InitializeHistory()
    {
        if (!File.Exists(_historyPath) || new FileInfo(_historyPath).Length == 0)
        {
            using var fs = File.Create(_historyPath);
            using var writer = new BinaryWriter(fs);
            writer.Write(Header);
            writer.Write(HistoryHeader);
            writer.Write(SchemaVersion);
            writer.Write(0);
        }
    }
    
    private static void InitializeSearchQueries()
    {
        if (!File.Exists(_searchQueriesPath) || new FileInfo(_searchQueriesPath).Length == 0)
        {
            using var fs = File.Create(_searchQueriesPath);
            using var writer = new BinaryWriter(fs);
            writer.Write(Header);
            writer.Write(SearchQueriesHeader);
            writer.Write(SchemaVersion);
            writer.Write(0);
        }
    }
}
