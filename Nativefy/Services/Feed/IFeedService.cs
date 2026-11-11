using Nativefy.Engine;
using System.Threading.Tasks;

namespace Nativefy.Services.Feed;

public interface IFeedService
{
    Task<List<FeedResult>> GetFeedAsync();
}
