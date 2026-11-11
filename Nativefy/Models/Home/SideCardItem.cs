namespace Nativefy.Models.Home;

public sealed class SideCardItem(string id, string imageUrl, string eyebrow, string title, string author)
{
    public string Id { get; set; } = id;
    public string ImageUrl { get; set; } = imageUrl;
    public string Eyebrow { get; set; } = eyebrow;
    public string Title { get; set; } = title;
    public string Author { get; set; } = author;
}
