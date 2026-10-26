namespace Nativefy.Models;

public sealed class PlaylistNavItem
{
    public string ImageUrl { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Id { get; set; } = null!;
    public PlaylistNavItemType Type { get; set; }
    public List<PlaylistNavItem>? Children { get; set; } = [];
}

public enum PlaylistNavItemType
{
    DynamicClosedSeparator,
    NewPlaylist,
    NewFolder
}