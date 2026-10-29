namespace Nativefy.Engine;

public sealed class NativefyEngine(SearchEngine search, PlaylistEngine playlist) : IDisposable
{
    public SearchEngine Search { get; } = search;
    public PlaylistEngine Playlist { get; } = playlist;

    public void Dispose()
    {
        Search.Dispose();
    }
}
