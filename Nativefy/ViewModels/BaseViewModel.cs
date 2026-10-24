using CommunityToolkit.Mvvm.ComponentModel;

namespace Nativefy.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial bool IsNotLoading { get; set; } = false;

    partial void OnIsLoadingChanged(bool value)
    {
        IsNotLoading = !IsLoading;
    }
}
