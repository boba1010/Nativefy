using Nativefy.Engine.Search;
using TubeSharp;
using TubeSharp.Search;

namespace Nativefy.Engine;

public sealed class SearchEngine(YoutubeClient youtube)
{
    public async Task<List<Search.ISearchResult>> GetResultAsync(string query, int maxResults = 25, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || maxResults <= 0)
            return [];

        var results = new List<Search.ISearchResult>();
        var uniqueIds = new HashSet<string>();

        await foreach (var result in youtube.Search.GetSearchResultRangeAsync(query, cancellationToken))
        {
            switch (result)
            {
                case VideoSearchResult video:
                    if (uniqueIds.Add($"video:{video.Id}"))
                        results.Add(new Video(video));
                    break;

                case PlaylistSearchResult playlist:
                    if (uniqueIds.Add($"playlist:{playlist.Id}"))
                        results.Add(new Playlist(playlist));
                    break;

                case ChannelSearchResult channel:
                    if (uniqueIds.Add($"channel:{channel.Id}"))
                        results.Add(new Channel(channel));
                    break;
            }

            if (results.Count >= maxResults)
                break;
        }

        return results;
    }
}