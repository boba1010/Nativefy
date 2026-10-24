using Nativefy.Enums;

namespace Nativefy.Models.Home;

public sealed class HomeSection
{
    public string Title { get; set; } = null!;
    public string SectionUri { get; set; } = null!;
    public List<MediaCardItem> Items { get; set; } = [];
    public ViewType ViewType { get; set; }
}
