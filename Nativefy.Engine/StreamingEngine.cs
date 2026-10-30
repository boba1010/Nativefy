using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Nativefy.Engine;

public sealed class StreamingEngine(YoutubeClient youtube)
{
    public async Task<string?> StartAudioPlaybackAsync(string videoId)
    {
        try
        {
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (audioStreamInfo != null)
            {
                string streamingUrl = audioStreamInfo.Url;
                return streamingUrl;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to stream YouTube audio: {ex.Message}");
            return null;
        }
    }
}