namespace Nativefy.Models;

public sealed class MediaCardItem
{
    public string Title { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string Artist { get; set; } = null!;

    public string GetSubtitleText() => $"{Title} • {Artist}";
}
