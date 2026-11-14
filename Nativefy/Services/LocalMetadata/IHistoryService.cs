using Nativefy.Models.Media;

namespace Nativefy.Services.LocalMetadata;

public interface IHistoryService
{
    void UpdateOrAdd(HistoryItem item);
    List<HistoryItem> GetItems();
    HistoryItem? GetById(string id);
}
