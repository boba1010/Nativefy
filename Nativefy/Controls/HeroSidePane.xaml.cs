namespace Nativefy.Controls;

public sealed partial class HeroSidePane : UserControl
{
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(HeroSidePane), new PropertyMetadata(true, OnIsLoadingPropertyChanged));

    public bool IsNotLoading
    {
        get => (bool)GetValue(IsNotLoadingProperty);
        set => SetValue(IsNotLoadingProperty, value);
    }

    private static readonly DependencyProperty IsNotLoadingProperty =
        DependencyProperty.Register(nameof(IsNotLoading), typeof(bool), typeof(HeroSidePane), new PropertyMetadata(null));
    
    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(HeroSidePane), new PropertyMetadata(null));

    public event EventHandler<object, RoutedEventArgs>? ItemInvoked;

    public HeroSidePane()
    {
        InitializeComponent();
    }

    public void OnButtonCLick(object sender, RoutedEventArgs e)
    {
        ItemInvoked?.Invoke(sender, e);
    }

    private static void OnIsLoadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeroSidePane control && e.NewValue is bool isLoading)
            control.IsNotLoading = !isLoading;
    }
}
