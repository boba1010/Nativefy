using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Playlist = Nativefy.Models.Media.Playlist;

namespace Nativefy.Services.Playback;

public sealed class MediaLoader(IStreamingBackend streaming)
{
    public async Task<MediaPlaybackItem> CreatePlaybackItemAsync(
        MediaItem item,
        CancellationToken cancellationToken = default,
        Resolution resolution = Resolution.Standard360p)
    {
        ArgumentNullException.ThrowIfNull(item);

        MediaSource source = await streaming.CreateSourceAsync(item, resolution, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

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
        ArgumentNullException.ThrowIfNull(itemAdded);

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
