namespace Nativefy.Controls;

public sealed partial class FilterCollections : UserControl
{
    public event EventHandler<object, RoutedEventArgs>? FilterClicked;

    public FilterCollections()
    {
        InitializeComponent();
    }

    private void OnFilterButtonClick(object sender, RoutedEventArgs e)
    {
        FilterClicked?.Invoke(sender, e);
    }

    public object FilterCollectionsSource
    {
        get => GetValue(FilterCollectionsSourceProperty);
        set => SetValue(FilterCollectionsSourceProperty, value);
    }

    private static DependencyProperty FilterCollectionsSourceProperty =
        DependencyProperty.Register(
            nameof(FilterCollectionsSource),
            typeof(object),
            typeof(FilterCollections),
            new PropertyMetadata(null));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private static DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(FilterCollections),
            new PropertyMetadata(true, OnIsLoadingChanged));

    private bool IsNotLoading
    {
        get => (bool)GetValue(IsNotLoadingProperty);
        set => SetValue(IsNotLoadingProperty, value);
    }

    private static DependencyProperty IsNotLoadingProperty =
        DependencyProperty.Register(
            nameof(IsNotLoading),
            typeof(bool),
            typeof(FilterCollections),
            new PropertyMetadata(null));

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is  FilterCollections control && e.NewValue is bool isLoading)
            control.IsNotLoading = !isLoading;
    }
}
