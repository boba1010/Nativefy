using CommunityToolkit.Mvvm.ComponentModel;
using Nativefy.Models.Home;
using System.Collections.ObjectModel;

namespace Nativefy.ViewModels.Home;

public sealed partial class HeroAdapter : ObservableObject
{
    public ObservableCollection<HeroCarouselItem> HeroSlides { get; } = [];

    public ObservableCollection<SideCardItem> Cards { get; } = [];
}
