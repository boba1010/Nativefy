using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Xaml.Media.Animation;
using Nativefy.Models.Home;
using Color = Windows.UI.Color;

namespace Nativefy.Controls;

public sealed partial class MediaShelf : UserControl
{
    public event EventHandler<object, RoutedEventArgs>? PlayClick;
    public event EventHandler<object, RoutedEventArgs>? CardClick;

    public MediaShelf()
    {
        InitializeComponent();
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
            typeof(MediaShelf),
            new PropertyMetadata(true, OnIsLoadingPropertyChanged));

    private static void OnIsLoadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MediaShelf control && e.NewValue is bool isLoading)
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
            typeof(MediaShelf),
            new PropertyMetadata(null));

    public object MediaSource
    {
        get => GetValue(MediaSourceProperty);
        set => SetValue(MediaSourceProperty, value);
    }

    private static DependencyProperty MediaSourceProperty =
        DependencyProperty.Register(
            nameof(MediaSource),
            typeof(object),
            typeof(MediaShelf),
            new PropertyMetadata(null));

    public string ShelfTitle
    {
        get => (string)GetValue(ShelfTitleProperty);
        set => SetValue(ShelfTitleProperty, value);
    }

    private static DependencyProperty ShelfTitleProperty =
        DependencyProperty.Register(
            nameof(ShelfTitle),
            typeof(string),
            typeof(MediaShelf),
            new PropertyMetadata(null));

    public new string Tag
    {
        get => (string)GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }

    private static new DependencyProperty TagProperty =
        DependencyProperty.Register(
            nameof(Tag),
            typeof(string),
            typeof(MediaShelf),
            new PropertyMetadata(null));

    private void OnPlayButtonClick(object sender, RoutedEventArgs e)
    {
        PlayClick?.Invoke(sender, e);
    }

    private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid cardGrid && cardGrid.FindName("hoverOverlay") is Grid overlay)
        {
            Color hoverColor = (Color)Application.Current.Resources["CardBackgroundFillColorDefault"];

            var colorAnim = new Microsoft.UI.Xaml.Media.Animation.ColorAnimation
            {
                To = hoverColor,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            Storyboard.SetTarget(colorAnim, cardGrid);
            Storyboard.SetTargetProperty(colorAnim, "(Grid.Background).(SolidColorBrush.Color)");

            var storyboard = new Storyboard();
            storyboard.Children.Add(colorAnim);
            storyboard.Begin();

            AnimationBuilder.Create()
                            .Opacity(to: 1.0, duration: TimeSpan.FromMilliseconds(200))
                            .Start(overlay);
        }
    }

    private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid cardGrid && cardGrid.FindName("hoverOverlay") is Grid overlay)
        {
            Color baseColor = (Color)Application.Current.Resources["CardBackgroundFillColorSecondary"];

            var colorAnim = new Microsoft.UI.Xaml.Media.Animation.ColorAnimation
            {
                To = baseColor,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            Storyboard.SetTarget(colorAnim, cardGrid);
            Storyboard.SetTargetProperty(colorAnim, "(Grid.Background).(SolidColorBrush.Color)");

            var storyboard = new Storyboard();
            storyboard.Children.Add(colorAnim);
            storyboard.Begin();

            AnimationBuilder.Create()
                            .Opacity(to: 0.0, duration: TimeSpan.FromMilliseconds(200))
                            .Start(overlay);
        }
    }

    private void Card_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var properties = e.GetCurrentPoint(sender as UIElement).Properties;

        if (properties.IsLeftButtonPressed)
        {
            if (sender is FrameworkElement element && element.DataContext is HomeSection currentItem)
            {
                CardClick?.Invoke(sender, e);
            }
        }
    }
}
