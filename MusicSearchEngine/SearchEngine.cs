using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace Nativefy.Engine;

public sealed class SearchEngine : IDisposable
{
    private readonly YoutubeClient _youtube = new();

    public void Dispose()
    {
        _youtube.Dispose();
    }

    public async Task<List<IPlayableItem>> GetResultAsync(string query, int maxResults = 25)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var results = new List<IPlayableItem>();

        var uniqueIds = new HashSet<string>();

        await foreach (var result in _youtube.Search.GetResultsAsync(query))
        {
            if (results.Count >= maxResults)
                break;

            if (result is VideoSearchResult video && uniqueIds.Add(video.Id))
                results.Add(new PlayableMedia(video));
            else if (result is PlaylistSearchResult playlist && uniqueIds.Add(playlist.Id))
                results.Add(new PlayablePlaylist(playlist));
        }

        return results;
    }
}

#region Unified UI Models

public interface IPlayableItem
{
    string Id { get; }
    string Title { get; }
    string ImageUrl { get; }
    string Author { get; }
    string Url { get; }
    TimeSpan? Duration { get; }
    bool IsPlaylist { get; }
}

public sealed class PlayableMedia : IPlayableItem
{
    public string Id { get; }
    public string Title { get; }
    public string ImageUrl { get; }
    public string Url { get; }
    public string Author { get; }
    public TimeSpan? Duration { get; }
    public bool IsPlaylist => false;

    // Constructor for live searches
    public PlayableMedia(VideoSearchResult video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnails.GetWithHighestResolution().Url;
        Url = video.Url;
        Author = video.Author.ChannelTitle;
        Duration = video.Duration;
    }

    // Constructor for database restoration
    public PlayableMedia(PlayableItemDto dto)
    {
        Id = dto.Id;
        Title = dto.Title;
        ImageUrl = dto.ImageUrl;
        Url = dto.Url;
        Author = dto.Author;
        Duration = dto.Duration;
    }
}

public sealed class PlayablePlaylist : IPlayableItem
{
    public string Id { get; }
    public string Title { get; }
    public string ImageUrl { get; }
    public string Url { get; }
    public string Author { get; }
    public TimeSpan? Duration => null;
    public bool IsPlaylist => true;

    // Constructor for live searches
    public PlayablePlaylist(PlaylistSearchResult playlist)
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

    // Constructor for database restoration
    public PlayablePlaylist(PlayableItemDto dto)
    {
        Id = dto.Id;
        Title = dto.Title;
        ImageUrl = dto.ImageUrl;
        Url = dto.Url;
        Author = dto.Author;
    }
}

#endregion
