namespace Nativefy.Engine.Playable;

public interface IPlayable
{
    string Id { get; }
    string Title { get; }
    string ImageUrl { get; }
    string Author { get; }
    string Url { get; }
    List<string> Keywords { get; }
    string? Description { get; }

    string GetSearchableText();
}
