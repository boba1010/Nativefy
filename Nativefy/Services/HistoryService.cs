using Nativefy.DbContexts;
using Nativefy.Models.Media;

namespace Nativefy.Services;

public sealed class HistoryService : IHistoryService
{
    public void AddItemToHistory(MediaItem item)
    {
        if (NativefyDatabaseProvider.History.Any(v => v.Id == item.Id))
            return;
        NativefyDatabaseProvider.History.Add(item);
    }

    public List<MediaItem> GetItems()
    {
        return NativefyDatabaseProvider.History;
    }
}
