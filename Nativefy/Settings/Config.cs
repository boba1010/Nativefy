using System.Text.Json;
using System.Text.Json.Serialization;
using Path = System.IO.Path;

namespace Nativefy.Settings;

public static partial class Config
{
    // cache size in MB

    private static readonly string _path = "";

    public const int MinMediaCacheSize = 1024;
    public const int MaxMediaCacheSize = 102_400;

    public static int MediaCacheSize { get; set; } = 5120;

    public static bool DeleteDuplicateVideosWithDifferentResolutions { get; set; } = true;

    static Config()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appFolder = Path.Combine(localAppData, "Nativefy");
        Directory.CreateDirectory(appFolder);

        _path = Path.Combine(appFolder, "nativefy.config");

        if (File.Exists(_path))
        {
            using var fs = File.OpenRead(_path);

            var config = JsonSerializer.Deserialize(fs, ConfigJsonContext.Default.ConfigDto) ?? new(5120, true);
            MediaCacheSize = config.MediaCacheSize;
            DeleteDuplicateVideosWithDifferentResolutions = config.DeleteDuplicateVideosWithDifferentResolutions;
        }
    }

    public static void SaveConfig()
    {
        using var fs = File.Create(_path);

        var config = new ConfigDto(MediaCacheSize, DeleteDuplicateVideosWithDifferentResolutions);

        JsonSerializer.Serialize(fs, config, ConfigJsonContext.Default.ConfigDto);
    }

    private sealed class ConfigDto(int mediaCacheSize = 5120, bool deleteDuplicateVideosWithDifferentResolutions = true)
    {
        public int MediaCacheSize { get; set; } = mediaCacheSize;
        public bool DeleteDuplicateVideosWithDifferentResolutions { get; set; } = deleteDuplicateVideosWithDifferentResolutions;
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfigDto))]
    private sealed partial class ConfigJsonContext : JsonSerializerContext;
}