namespace Nativefy.Engine.Channels;

public sealed record class ChannelTab(string Title, string BrowseId)
{
    public bool IsSelected { get; set; }
}
