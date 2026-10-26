using Nativefy.Models.Home;

namespace Nativefy;

public partial class ShelfTemplateSelector : DataTemplateSelector
{
    public DataTemplate StandardTemplate { get; set; } = null!;
    public DataTemplate HeroPromptTemplate { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is HomeSection section)
        {
            if (section.ViewType == Enums.ViewType.HeroPrompt)
                return HeroPromptTemplate;
        }

        return StandardTemplate;
    }
}

public class PlaylistTemplateSelector : DataTemplateSelector
{
    public DataTemplate NormalTemplate { get; set; } = null!;
    public DataTemplate SeparatorTemplate { get; set; } = null!;
    public DataTemplate AddButtonTemplate { get; set; } = null!;
    public DataTemplate CompactNormalTemplate { get; set; } = null!;

    public bool IsPaneOpen { get; set; } = true;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is PlaylistNavItem model)
        {
            if (!IsPaneOpen)
            {
                return model.Type switch
                {
                    PlaylistNavItemType.DynamicClosedSeparator => SeparatorTemplate,
                    PlaylistNavItemType.NewPlaylist or PlaylistNavItemType.NewFolder => AddButtonTemplate,
                    _ => CompactNormalTemplate
                };
            }

            return model.Type switch
            {
                PlaylistNavItemType.DynamicClosedSeparator => SeparatorTemplate,
                PlaylistNavItemType.NewPlaylist or PlaylistNavItemType.NewFolder => AddButtonTemplate,
                _ => NormalTemplate
            };
        }

        return NormalTemplate;
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}