using YoutubeExplode.Common;
using YoutubeExplode.Playlists;

namespace Nativefy.Engine.Playable;

public sealed class PlayablePlaylist : IPlaylist
{
    public string Id { get; }
    public string Title { get; }
    public string ImageUrl { get; }
    public string Url { get; }
    public string Author { get; }
    public List<IMedia> Media { get; } = [];
    public List<string> Keywords { get; } = [];
    public string? Description { get; }

    public PlayablePlaylist(Playlist playlist, List<PlaylistVideo> videos)
    {
        Id = playlist.Id;
        Title = playlist.Title;
        Url = playlist.Url;
        Author = playlist.Author?.ChannelTitle ?? "Unknown Creator";
        ImageUrl = playlist.Thumbnails.GetWithHighestResolution().Url;
        Description = playlist.Description;

        foreach (var video in videos)
            Media.Add(new PlayableMedia(video));
    }

    public PlayablePlaylist(Search.Playlist playlist)
    {
        Id = playlist.Id;
        Title = playlist.Title;
        Url = playlist.Url;
        Author = playlist.Author;
        ImageUrl = playlist.ImageUrl;
    }

    public PlayablePlaylist(string id, string title, string url, string author, string imageUrl)
    {
        Id = id;
        Title = title;
        Url = url;
        Author = author;
        ImageUrl = imageUrl;
    }

    public string GetSearchableText()
    {
        string keywordString = string.Join(" ", Keywords);
        return $"{Title} {keywordString}".ToLower();
    }
}