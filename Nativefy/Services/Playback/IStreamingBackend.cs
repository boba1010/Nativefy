using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace Nativefy.Services.Playback;

public interface IStreamingBackend
{
    Task<MediaSource> CreateSourceAsync(MediaItem item, Resolution resolution, CancellationToken cancellationToken = default);
}
