using System.Text.Json;
using System.Text.Json.Serialization;
using Path = System.IO.Path;

namespace Nativefy.Settings;

public enum Browser
{
    None,
    MicrosoftEdge,
    GoogleChrome,
    MozillaFirefox,
    Brave
}

public static partial class Config
{
    // cache size in MB

    private static readonly string _path = "";

    public const int MinMediaCacheSize = 1024;
    public const int MaxMediaCacheSize = 102_400;

    public static int MediaCacheSize { get; set; } = 5120;

    public static bool DeleteDuplicateVideosWithDifferentResolutions { get; set; } = true;

    public static Browser Browser { get; set; } = Browser.None;

    static Config()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appFolder = Path.Combine(localAppData, "Nativefy");
        Directory.CreateDirectory(appFolder);

        _path = Path.Combine(appFolder, "nativefy.config");

        if (File.Exists(_path))
        {
            using var fs = File.OpenRead(_path);

            var config = JsonSerializer.Deserialize(fs, ConfigJsonContext.Default.ConfigDto) ?? new();
            MediaCacheSize = config.MediaCacheSize;
            DeleteDuplicateVideosWithDifferentResolutions = config.DeleteDuplicateVideosWithDifferentResolutions;
            Browser = config.Browser;
        }
    }

    public static void SaveConfig()
    {
        using var fs = File.Create(_path);

        var config = new ConfigDto(MediaCacheSize, DeleteDuplicateVideosWithDifferentResolutions, Browser);

        JsonSerializer.Serialize(fs, config, ConfigJsonContext.Default.ConfigDto);
    }

    private sealed class ConfigDto(int mediaCacheSize = 5120, bool deleteDuplicateVideosWithDifferentResolutions = true, Browser browser = Browser.None)
    {
        public int MediaCacheSize { get; set; } = mediaCacheSize;

        public bool DeleteDuplicateVideosWithDifferentResolutions { get; set; } = deleteDuplicateVideosWithDifferentResolutions;

        public Browser Browser { get; set; } = browser;
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfigDto))]
    private sealed partial class ConfigJsonContext : JsonSerializerContext;
}