using System.Windows.Input;

namespace Nativefy.Models;

public sealed class HeroCarouselItem(string title, string author, string primaryCtaText, Uri? imageUri = null, ICommand? primaryCtaCommand = null, object? primaryCtaCommandParams = null)
{
    public string Title { get; set; } = title;
    public string Author { get; set; } = author;
    public Uri? ImageUri { get; set; } = imageUri;
    public string? ImageUrl => ImageUri?.ToString();
    public string PrimaryCtaText { get; set; } = primaryCtaText;
    public ICommand? PrimaryCtaCommand { get; set; } = primaryCtaCommand;
    public object? PrimaryCtaCommandParameter { get; set; } = primaryCtaCommandParams;
}