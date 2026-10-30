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

    [Description("1080p Premium")]
    FullHd1080pPremium,

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

    // 8K / 4320p
    [Description("4320p")]
    Uh8k4320p,

    [Description("4320p60")]
    Uh8k4320p60
}


public sealed class StreamingEngine(YoutubeClient youtube)
{
    public async Task<(string? Url, string? AudioUrl, bool IsMuxed)> StartPlaybackAsync(string videoId, Resolution resolution)
    {
        try
        {
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var label = resolution.GetLabel();

            // Check for exact Muxed stream match (720p or lower)
            var muxedStreams = streamManifest.GetMuxedStreams().ToList();
            var exactMuxed = muxedStreams
                .FirstOrDefault(s => s.VideoQuality.Label.Equals(label, StringComparison.OrdinalIgnoreCase));

            if (exactMuxed != null)
                return (exactMuxed.Url, null, IsMuxed: true);

            // Check for exact Adaptive Video match (1080p, 4K, etc.)
            var videoOnlyStreams = streamManifest.GetVideoOnlyStreams().ToList();
            var exactAdaptiveVideo = videoOnlyStreams
                .FirstOrDefault(s => s.VideoQuality.Label.Equals(label, StringComparison.OrdinalIgnoreCase));

            var highestAudio = streamManifest.GetAudioStreams().GetWithHighestBitrate();

            if (exactAdaptiveVideo != null && highestAudio != null)
                return (exactAdaptiveVideo.Url, highestAudio.Url, IsMuxed: false);

            // Fallback: Get the highest available Muxed stream if exact matching failed entirely
            var anyMuxedStream = muxedStreams.GetWithHighestVideoQuality();
            if (anyMuxedStream != null)
                return (anyMuxedStream.Url, null, IsMuxed: true);

            // Fallback 2: return highest adaptive video + audio if no muxed exist at all
            var highestAdaptiveVideo = videoOnlyStreams.GetWithHighestVideoQuality();
            if (highestAdaptiveVideo != null && highestAudio != null)
                return (highestAdaptiveVideo.Url, highestAudio.Url, IsMuxed: false);

            // Fallback for Audio-Only YouTube items (e.g. YouTube Music tracks)
            if (highestAudio != null)
                return (null, highestAudio.Url, IsMuxed: false);

            return (null, null, false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to stream YouTube Video: {ex.Message}");
            return (null, null, false);
        }
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