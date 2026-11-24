using Nativefy.Engine.Playable;

namespace Nativefy.Engine.Channels;

public sealed record ChannelVideosPage(IReadOnlyList<PlayableMedia> Videos, string? ContinuationToken = null);