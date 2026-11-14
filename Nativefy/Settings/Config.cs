namespace Nativefy.Settings;

public static class Config
{
    // cache size in MB

    public const int MinMediaCacheSize = 1024;
    public const int MaxMediaCacheSize = 102_400;

    public static int MediaCacheSize { get; set; } = 5120;

    public static bool DeleteDuplicateVideosWithDifferentResolutions { get; set; } = true;
}