using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace Nativefy.Engine.Search;

public sealed class Playlist : ISearchResult
{
    public string Id { get; }

    public string Title { get; }

    public string ImageUrl { get; }

    public string Author { get; }

    public string Url { get; }

    public Playlist(PlaylistSearchResult playlist)
    {
        Id = playlist.Id;
        Title = playlist.Title;
        Url = playlist.Url;
        Author = playlist.Author?.ChannelTitle ?? "Unknown Creator";
        try
        {
            ImageUrl = playlist.Thumbnails.GetWithHighestResolution().Url ?? "playlist.png";
        }
        catch (Exception)
        {
            ImageUrl = "playlist.png";
        }
    }
}
