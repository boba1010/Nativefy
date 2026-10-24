using System.Windows.Input;

namespace Nativefy.Models;

public sealed class HeroCarouselItem
{
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string TaglineIconGlyph { get; set; } = string.Empty;
    public Color TaglineIconColor { get; set; }
    public Uri? ImageUri { get; set; }
    public string? ImageUrl => ImageUri?.ToString();
    public Color Accent { get; set; }
    public string PrimaryCtaText { get; set; } = string.Empty;
    public ICommand? PrimaryCtaCommand { get; set; }
    public object? PrimaryCtaCommandParameter { get; set; }
    public string SecondaryCtaText { get; set; } = string.Empty;
    public ICommand? SecondaryCtaCommand { get; set; }
    public object? SecondaryCtaCommandParameter { get; set; }
    public object? Tag { get; set; }
    public bool UseImageAsBackground { get; set; }
}