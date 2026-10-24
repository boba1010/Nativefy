using CommunityToolkit.Mvvm.ComponentModel;
using Nativefy.Models.Home;
using System.Collections.ObjectModel;

namespace Nativefy.ViewModels.Home;

public sealed partial class HeroAdapter : BaseViewModel
{
    public ObservableCollection<HeroCarouselItem> HeroSlides { get; } = [];

    [ObservableProperty]
    public partial SideCardItem Card0 { get; set; }

    [ObservableProperty]
    public partial SideCardItem Card1 { get; set; }

    [ObservableProperty]
    public partial SideCardItem Card2 { get; set; }
}
