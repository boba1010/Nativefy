namespace Nativefy.Controls;

public sealed partial class HomeHeader : UserControl
{
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(HomeHeader), new PropertyMetadata(true, OnIsLoadingPropertyPropertyChanged));

    private bool IsNotLoading
    {
        get => (bool)GetValue(IsNotLoadingProperty);
        set => SetValue(IsNotLoadingProperty, value);
    }

    public static readonly DependencyProperty IsNotLoadingProperty =
        DependencyProperty.Register(nameof(IsNotLoading), typeof(bool), typeof(HomeHeader), new PropertyMetadata(null));

    private static void OnIsLoadingPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HomeHeader control && e.NewValue is bool isLoading)
            control.IsNotLoading = !isLoading;
    }

    public event EventHandler<object, RoutedEventArgs>? FilterChecked;

    private void OnRadioFilterButtonChecked(object sender, RoutedEventArgs e)
    {
        FilterChecked?.Invoke(sender, e);
    }

    public HomeHeader()
    {
        InitializeComponent();
    }
}
