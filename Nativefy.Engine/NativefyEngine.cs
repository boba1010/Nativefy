using TubeSharp;

namespace Nativefy.Engine;

public sealed class NativefyEngine : IDisposable
{
    private readonly YoutubeClient _youtube;

    public SearchEngine Search { get; }
    public PlaylistEngine Playlist { get; }
    public FeedEngine Feed { get; }
    public ChannelEngine Channel { get; }

    public NativefyEngine()
    {
        _youtube = new();

        Search = new(_youtube);
        Playlist = new(_youtube);
        Feed = new(Search);
        Channel = new(_youtube);
    }

    public void Dispose()
    {
        _youtube.Dispose();
    }
}
