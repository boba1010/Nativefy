using CommunityToolkit.Mvvm.ComponentModel;
using Nativefy.Models.Home;
using Nativefy.ViewModels.Home;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class HomeViewModel : BaseViewModel
{
    public ObservableCollection<HomeSection> Sections { get; } = [];
    
    [ObservableProperty]
    public partial HeroAdapter HeroAdapter { get; set; } = new();

    public async Task LoadHomeFeedAsync()
    {

    }
}
