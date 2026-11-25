using Nativefy.Engine.Playable;

namespace Nativefy.Engine.Channels;

public sealed record ChannelHomePage(IReadOnlyList<ChannelHomeShelf> Shelves);

public sealed record ChannelHomeShelf(string Title, string? Subtitle, string? Url, List<PlayableMedia> Videos);
