using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace Nativefy.Engine.Search;

public sealed class Channel(ChannelSearchResult channel) : ISearchResult
{
    public string Id { get; } = channel.Id;

    public string ImageUrl { get; } = channel.Thumbnails.GetWithHighestResolution().Url.Insert(0, "https:");

    public string Url { get; } = channel.Url;

    public string Title { get; } = channel.Title;
}
