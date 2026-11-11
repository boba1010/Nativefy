using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Nativefy.Engine;

public enum Resolution
{
    // Low / Data Saver Qualities
    [Description("144p")]
    Low144p,

    [Description("240p")]
    Low240p,

    [Description("360p")]
    Standard360p,

    [Description("480p")]
    Standard480p,

    // High Definition (HD)
    [Description("720p")]
    Hd720p,

    [Description("720p60")]
    Hd720p60,

    [Description("1080p")]
    FullHd1080p,

    [Description("1080p60")]
    FullHd1080p60,

    // Ultra HD / 2K
    [Description("1440p")]
    Qhd1440p,

    [Description("1440p60")]
    Qhd1440p60,

    // 4K / 2160p
    [Description("2160p")]
    Uh4k2160p,

    [Description("2160p60")]
    Uh4k2160p60,
}


public sealed class StreamingEngine(YoutubeClient youtube)
{
    public async Task<(string? Url, string? AudioUrl)> GetPlaybackUrlsAsync(string videoId, Resolution resolution)
    {
        try
        {
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var label = resolution.GetLabel();

            // Check for exact Adaptive Video match (1080p, 4K, etc.)
            var videoOnlyStreams = streamManifest.GetVideoOnlyStreams().ToList();
            var exactAdaptiveVideo = videoOnlyStreams
                .FirstOrDefault(s => s.VideoQuality.Label.Equals(label, StringComparison.OrdinalIgnoreCase));

            var highestAudio = streamManifest.GetAudioStreams().GetWithHighestBitrate();

            if (exactAdaptiveVideo != null && highestAudio != null)
                return (exactAdaptiveVideo.Url, highestAudio.Url);

            // Fallback 2: return highest adaptive video + audio if no muxed exist at all
            var highestAdaptiveVideo = videoOnlyStreams.GetWithHighestVideoQuality();
            if (highestAdaptiveVideo != null && highestAudio != null)
                return (highestAdaptiveVideo.Url, highestAudio.Url);

            // Fallback for Audio-Only YouTube items (e.g. YouTube Music tracks)
            if (highestAudio != null)
                return (null, highestAudio.Url);

            return (null, null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to stream YouTube Video: {ex.Message}");
            return (null, null);
        }
    }

    public async Task<Resolution[]> GetResolutionsAsync(string id, CancellationToken cancellationToken = default)
    {
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(id, cancellationToken);

        var streams = streamManifest.GetVideoOnlyStreams().ToList();

        List<Resolution> resolutions = [];

        foreach (var resolution in Enum.GetValues<Resolution>())
        {
            var label = resolution.GetLabel();

            if (streams.Any(s => s.VideoQuality.Label == label))
                resolutions.Add(resolution);
        }

        return [.. resolutions];
    }
}

public static class YoutubeQualityExtensions
{
    public static string GetLabel(this Resolution res)
    {
        FieldInfo fi = res.GetType().GetField(res.ToString())!;
        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : res.ToString();
    }
}