using Nativefy.Engine;
using Nativefy.Services.LocalMetadata;
using System.Threading.Tasks;

namespace Nativefy.Services.Feed;

public sealed class HomeFeedService(NativefyEngine nativefy, ISearchQueriesService savedSearch) : IFeedService
{
    public async Task<List<FeedResult>> GetFeedAsync()
    {
        var queries = savedSearch.GetQueries();

        return await nativefy.Feed.GenerateFeedAsync(queries);
    }
}
