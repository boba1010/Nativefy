using Nativefy.Engine.Playable;
using Nativefy.Models.Media;

namespace Nativefy;

public partial class SearchTemplateSelector : DataTemplateSelector
{
    public DataTemplate MediaTemplate { get; set; } = null!;
    public DataTemplate PlaylistTemplate { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is IPlaylist)
            return PlaylistTemplate;

        return MediaTemplate;
    }
}

public partial class HistoryTemplateSelector : DataTemplateSelector
{
    public DataTemplate MediaTemplate { get; set; } = null!;
    public DataTemplate PlaylistTemplate { get; set; } = null!;
    public DataTemplate SearchTemplate { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is HistoryPlaylist)
            return PlaylistTemplate;
        else if (item is HistoryMediaItem)
            return MediaTemplate;
        else if (item is SearchHistory)
            return SearchTemplate;

        return null!;
    }
}