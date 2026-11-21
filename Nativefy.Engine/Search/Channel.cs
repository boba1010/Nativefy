using TubeSharp.Search;

namespace Nativefy.Engine.Search;

public sealed class Channel(ChannelSearchResult channel) : ISearchResult
{
    public string Id { get; } = channel.Id;

    public string ImageUrl { get; } = channel.AvatarUrl ?? "channel.png";

    public string Url { get; } = channel.Url;

    public string Title { get; } = channel.Title;

    public string Subscribers { get; } = channel.Subscribers;

    public string Handle { get; } = channel.Handle;
}
