using Nativefy.Models.Media;

namespace Nativefy.Services;

public interface IHistoryService
{
    void AddItemToHistory(MediaItem item);
    List<MediaItem> GetItems();
}
