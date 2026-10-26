namespace Nativefy.Engine;

public sealed class NativefyEngine(SearchEngine search) : IDisposable
{
    public SearchEngine Search { get; } = search;

    public void Dispose()
    {
        Search.Dispose();
    }
}
