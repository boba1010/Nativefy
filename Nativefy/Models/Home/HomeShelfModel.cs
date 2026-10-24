using Nativefy.Enums;

namespace Nativefy.Models.Home;

public sealed class HomeShelfModel
{
    public ViewType ViewType { get; set; }
    public List<HomeSection> Sections { get; set; } = [];
}
