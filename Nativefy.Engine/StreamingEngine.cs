using System.ComponentModel;
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
}


public sealed class StreamingEngine(YoutubeClient youtube)
{
    public async Task<string[]> StartPlaybackAsync(string videoId, Resolution resolution, CancellationToken cancellationToken = default)
    {
        try
        {
            string[] urls = new string[2];

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
            var label = resolution.GetLabel();

            // Check for exact Adaptive Video match (1080p, 4K, etc.)
            var videoOnlyStreams = streamManifest.GetVideoOnlyStreams().ToList();
            var exactAdaptiveVideo = videoOnlyStreams
                .FirstOrDefault(s => s.VideoQuality.Label.Equals(label, StringComparison.OrdinalIgnoreCase));

            var highestAudio = streamManifest.GetAudioStreams().GetWithHighestBitrate();

            if (exactAdaptiveVideo != null && highestAudio != null)
            {
                urls[0] = exactAdaptiveVideo.Url;
                urls[1] = highestAudio.Url;
                return urls;
            }

            // Fallback 2: return highest adaptive video + audio if no muxed exist at all
            var highestAdaptiveVideo = videoOnlyStreams.GetWithHighestVideoQuality();
            if (highestAdaptiveVideo != null && highestAudio != null)
            {
                urls[0] = highestAdaptiveVideo.Url;
                urls[1] = highestAudio.Url;
                return urls;
            }

            // Fallback for Audio-Only YouTube items (e.g. YouTube Music tracks)
            if (highestAudio != null)
            {
                urls[1] = highestAudio.Url;
                return urls;
            }

            return [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to stream YouTube Video: {ex.Message}");
            return [];
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