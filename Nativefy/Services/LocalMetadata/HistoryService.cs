using Nativefy.DbContexts;
using Nativefy.Models.Media;

namespace Nativefy.Services.LocalMetadata;

public sealed class HistoryService : IHistoryService
{
    public void UpdateOrAdd(HistoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        int index = NativefyDatabaseProvider.History.FindIndex(v => v.Id == item.Id);

        item.UpdatedAt = DateTime.Now;

        if (index >= 0)
            NativefyDatabaseProvider.History[index] = item;
        else
        {
            item.AddedDate = DateTime.Now;
            NativefyDatabaseProvider.History.Add(item);
        }

        NativefyDatabaseProvider.SaveHistoryChanges();
    }

    public List<HistoryItem> GetItems()
    {
        return NativefyDatabaseProvider.History;
    }

    public HistoryItem? GetById(string id)
    {
        return NativefyDatabaseProvider.History.FirstOrDefault(i => i.Id == id);
    }
}
