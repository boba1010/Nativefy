
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Nativefy.Services;

public sealed class MediaLoader(Engine.NativefyEngine nativefy)
{
    public async Task<MediaPlaybackItem> CreatePlaybackItemAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        cancellationToken.ThrowIfCancellationRequested();

        var url = await nativefy.Streaming.StartAudioPlaybackAsync(item.Id);

        cancellationToken.ThrowIfCancellationRequested();

        if (!Uri.TryCreate(url, UriKind.Absolute, out var streamUrl))
            throw new InvalidOperationException($"Could not resolve a stream URL for '{item.Title}'.");

        var source = MediaSource.CreateFromUri(streamUrl);
        source.CustomProperties["Metadata"] = item;

        return new MediaPlaybackItem(source);
    }

    public async Task LoadRemainingAsync(
        Playlist playlist,
        MediaPlaybackList playbackList,
        int startIndex,
        Action itemAdded,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(playlist);
        ArgumentNullException.ThrowIfNull(playbackList);

        for (int i = startIndex; i < playlist.Items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var item = playlist.Items[i];

            try
            {
                var playbackItem = await CreatePlaybackItemAsync(item, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                playbackList.Items.Add(playbackItem);
                itemAdded();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load '{item.Title}': {ex}");
            }
        }
    }
}