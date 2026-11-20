namespace Nativefy.Models.Media;

public sealed class Channel()
{
    public string Id { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string ImageUrl { get; init; } = null!;
    public string Url { get; init; } = null!;
    public DateTime AddedDate { get; init; } = DateTime.Now;

    public Channel(string id, string title, string imageUrl, string url) : this()
    {
        Id = id;
        Title = title;
        ImageUrl = imageUrl;
        Url = url;
    }

    public Channel(Engine.Channels.Channel channel) : this()
    {
        Id = channel.Id;
        Title = channel.Title;
        ImageUrl = channel.ImageUrl;
        Url = channel.Url;
    }
}