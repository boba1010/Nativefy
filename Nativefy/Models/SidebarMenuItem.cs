using System.Windows.Input;

namespace Nativefy.Models;

public sealed class SidebarMenuItem
{
    public string Tag { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string? Glyph { get; set; }
    public string? ImageUrl { get; set; }

    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
    public bool HasGlyph => !HasImage && !string.IsNullOrEmpty(Glyph);

    public ICommand? RenameCommand { get; set; }
    public ICommand? DeleteCommand { get; set; }
}