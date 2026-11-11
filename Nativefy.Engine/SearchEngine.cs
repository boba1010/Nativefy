using YoutubeExplode;
using YoutubeExplode.Search;

namespace Nativefy.Engine;

public sealed class SearchEngine(YoutubeClient youtube)
{
    public async Task<List<Search.ISearchResult>> GetResultAsync(string query, int maxResults = 25, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || maxResults <= 0)
            return [];

        var results = new List<Search.ISearchResult>();
        var uniqueIds = new HashSet<string>();

        await foreach (var result in youtube.Search.GetResultsAsync(query, cancellationToken))
        {
            switch (result)
            {
                case VideoSearchResult video:
                    if (uniqueIds.Add($"video:{video.Id}"))
                        results.Add(new Search.Video(video));
                    break;

                case PlaylistSearchResult playlist:
                    if (uniqueIds.Add($"playlist:{playlist.Id}"))
                        results.Add(new Search.Playlist(playlist));
                    break;
            }

            if (results.Count >= maxResults)
                break;
        }

        return results;
    }
}