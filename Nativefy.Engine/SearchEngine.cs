using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;

namespace Nativefy.Engine;

public sealed class SearchEngine(YoutubeClient youtube)
{
    public async Task<List<IPlayable>> GetResultAsync(string query, int maxResults = 25)
    {
        if (string.IsNullOrWhiteSpace(query) || maxResults <= 0)
            return [];

        var results = new List<IPlayable>();
        var uniqueIds = new HashSet<string>();

        await foreach (var result in youtube.Search.GetResultsAsync(query))
        {
            switch (result)
            {
                case VideoSearchResult video:
                    if (uniqueIds.Add($"video:{video.Id}"))
                        results.Add(new PlayableMedia(video));
                    break;

                case PlaylistSearchResult playlist:
                    if (uniqueIds.Add($"playlist:{playlist.Id}"))
                        results.Add(new Playlist(playlist));
                    break;
            }

            if (results.Count >= maxResults)
                break;
        }

        return results;
    }
}

#region Unified UI Models

public interface IPlayable
{
    string Id { get; }
    string Title { get; }
    string ImageUrl { get; }
    string Author { get; }
    string Url { get; }
}

public interface IPlaylist : IPlayable
{
    public List<IMedia> Media { get; }
}

public interface IMedia : IPlayable
{
    TimeSpan Duration { get; }
}

public sealed class PlayableMedia : IMedia
{
    public string Id { get; }
    public string Title { get; }
    public string ImageUrl { get; }
    public string Url { get; }
    public string Author { get; }
    public TimeSpan Duration { get; }

    // Constructor for live searches
    public PlayableMedia(VideoSearchResult video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnails.GetWithHighestResolution().Url;
        Url = video.Url;
        Author = video.Author.ChannelTitle;
        if (video.Duration is not null)
            Duration = video.Duration.Value;
    }

    public PlayableMedia(PlaylistVideo video)
    {
        Id = video.Id;
        Title = video.Title;
        ImageUrl = video.Thumbnails.TryGetWithHighestResolution()?.Url ?? "image.png";
        Url = video.Url;
        Author = video.Author.ChannelTitle;
        if (video.Duration is not null)
            Duration = video.Duration.Value;
    }
}

public sealed class Playlist : IPlaylist
{
    public string Id { get; }
    public string Title { get; }
    public string ImageUrl { get; }
    public string Url { get; }
    public string Author { get; }
    public List<IMedia> Media { get; } = [];

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

    public Playlist(YoutubeExplode.Playlists.Playlist playlist, List<PlaylistVideo> videos)
    {
        Id = playlist.Id;
        Title = playlist.Title;
        Url = playlist.Url;
        Author = playlist.Author?.ChannelTitle ?? "Unknown Creator";
        ImageUrl = playlist.Thumbnails.GetWithHighestResolution().Url;

        foreach (var video in videos)
            Media.Add(new PlayableMedia(video));
    }
}

#endregion
