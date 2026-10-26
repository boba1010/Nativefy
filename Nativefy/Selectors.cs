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