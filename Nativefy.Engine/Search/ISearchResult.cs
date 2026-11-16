namespace Nativefy.Engine.Search;

public interface ISearchResult
{
    string Id { get; }
    string Title { get; }
    string ImageUrl { get; }
    string Url { get; }
}
