using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Threading;
using System.Threading.Tasks;

namespace Nativefy.Services.Playback;

public sealed record StreamUrls(string? Video, string? Audio);

public interface IStreamingBackend
{
    Task<StreamUrls> GetStreamUrlsAsync(MediaItem item, Resolution resolution, CancellationToken cancellationToken = default);
    Task<Resolution[]> GetResolutionsAsync(string id, CancellationToken cancellationToken = default);
}
