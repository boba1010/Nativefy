using CommunityToolkit.Mvvm.ComponentModel;

namespace Nativefy.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    public partial bool IsLoading { get; set; } = true;

    public bool IsNotLoading => !IsLoading;
}
