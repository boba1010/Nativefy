using Nativefy.Engine.Playable;

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