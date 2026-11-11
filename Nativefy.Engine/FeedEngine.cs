using Nativefy.Engine.Search;
using System.Runtime.InteropServices;

namespace Nativefy.Engine;

public sealed record class FeedResult(string Id, string Title, string Author, string ImageUrl, string Url, TimeSpan Duration);

public sealed class FeedEngine(SearchEngine searchEngine)
{
    public async Task<List<FeedResult>> GenerateFeedAsync(string[] searches, int maxResults = 30, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searches);

        var feed = new List<FeedResult>(maxResults * searches.Length);

        foreach (var query in searches)
        {
            var results = await searchEngine.GetResultAsync(query, maxResults, cancellationToken);
            foreach (var result in results)
            {
                if (result is Video video)
                    feed.Add(new(video.Id, video.Title, video.Author, video.ImageUrl, video.Url, video.Duration));
            }
        }

        feed = [.. feed.DistinctBy(f => f.Id)];

        Random.Shared.Shuffle(CollectionsMarshal.AsSpan(feed));

        return feed;
    }
}