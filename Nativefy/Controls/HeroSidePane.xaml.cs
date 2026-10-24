using Nativefy.Models.Home;

namespace Nativefy.Controls;

public sealed partial class HeroSidePane : UserControl
{
    public HeroSidePane()
    {
        InitializeComponent();
    }

    public event EventHandler<object, RoutedEventArgs>? ButtonClick;
    public void OnButtonCLick(object sender, RoutedEventArgs e)
    {
        ButtonClick?.Invoke(sender, e);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private static DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(HeroSidePane),
            new PropertyMetadata(true, OnIsLoadingPropertyChanged));

    private static void OnIsLoadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeroSidePane control && e.NewValue is bool isLoading)
        {
            control.IsNotLoading = !isLoading;
        }
    }

    public bool IsNotLoading
    {
        get => (bool)GetValue(IsNotLoadingProperty);
        set => SetValue(IsNotLoadingProperty, value);
    }

    private static DependencyProperty IsNotLoadingProperty =
        DependencyProperty.Register(
            nameof(IsNotLoading),
            typeof(bool),
            typeof(HeroSidePane),
            new PropertyMetadata(null));
    
    public SideCardItem Card0
    {
        get => (SideCardItem)GetValue(Card0Property);
        set => SetValue(Card0Property, value);
    }

    private static DependencyProperty Card0Property =
        DependencyProperty.Register(
            nameof(Card0),
            typeof(SideCardItem),
            typeof(HeroSidePane),
            new PropertyMetadata(null));
    public SideCardItem Card1
    {
        get => (SideCardItem)GetValue(Card1Property);
        set => SetValue(Card1Property, value);
    }

    private static DependencyProperty Card1Property =
        DependencyProperty.Register(
            nameof(Card1),
            typeof(SideCardItem),
            typeof(HeroSidePane),
            new PropertyMetadata(null));
    public SideCardItem Card2
    {
        get => (SideCardItem)GetValue(Card2Property);
        set => SetValue(Card2Property, value);
    }

    private static DependencyProperty Card2Property =
        DependencyProperty.Register(
            nameof(Card2),
            typeof(SideCardItem),
            typeof(HeroSidePane),
            new PropertyMetadata(null));
}
