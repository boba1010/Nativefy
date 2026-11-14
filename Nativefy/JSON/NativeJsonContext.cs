using Nativefy.Models.Media;
using System.Text.Json.Serialization;

namespace Nativefy.JSON;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HistoryMediaItem))]
[JsonSerializable(typeof(HistoryPlaylist))]
[JsonSerializable(typeof(LikedMediaPlaylist))]
[JsonSerializable(typeof(List<Playlist>))]
[JsonSerializable(typeof(List<LocalPlaylist>))]
[JsonSerializable(typeof(List<MediaItem>))]
[JsonSerializable(typeof(List<HistoryItem>))]
[JsonSerializable(typeof(List<string>))]
public sealed partial class NativefyJsonContext : JsonSerializerContext
{
}
