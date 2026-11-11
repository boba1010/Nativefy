namespace Nativefy.Models.Home;

public sealed class SideCardItem(string url, string imageUrl, string eyebrow, string title, string author)
{
    public string Url { get; set; } = url;
    public string ImageUrl { get; set; } = imageUrl;
    public string Eyebrow { get; set; } = eyebrow;
    public string Title { get; set; } = title;
    public string Author { get; set; } = author;
}
