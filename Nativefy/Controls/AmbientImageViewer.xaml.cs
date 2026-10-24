namespace Nativefy.Controls;

public sealed partial class AmbientImageViewer : UserControl
{
    private DispatcherTimer? _autoScrollTimer;

    public AmbientImageViewer()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // CRITICAL: Stop timer on unload to prevent severe background process memory leaks
        if (_autoScrollTimer != null)
        {
            _autoScrollTimer.Stop();
            _autoScrollTimer.Tick -= AutoScrollTimer_Tick!;
            _autoScrollTimer = null!;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeAutoScroll();
    }

    private void InitializeAutoScroll()
    {
        // 1. Initialize the timer
        _autoScrollTimer = new DispatcherTimer();

        // 2. Set the rotation interval (e.g., 5 seconds)
        _autoScrollTimer.Interval = TimeSpan.FromSeconds(5);

        // 3. Attach the tick event handler
        _autoScrollTimer.Tick += AutoScrollTimer_Tick!;

        // 4. Start the auto-rotation loop
        _autoScrollTimer.Start();
    }

    private void AutoScrollTimer_Tick(object sender, object e)
    {
        if (flipViewPipsPager == null || flipViewPipsPager.NumberOfPages <= 0) return;

        // Calculate next page index or loop back to 0 if at the end
        int nextIndex = flipViewPipsPager.SelectedPageIndex + 1;
        if (nextIndex >= flipViewPipsPager.NumberOfPages)
        {
            nextIndex = 0;
        }

        // Updating this automatically fires your existing MyPipsPager_SelectedIndexChanged layout animation
        flipViewPipsPager.SelectedPageIndex = nextIndex;
    }

    public bool IsLoading 
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(AmbientImageViewer),
            new PropertyMetadata(true, IsLoadingChanged));

    private static void IsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AmbientImageViewer control && e.NewValue is bool isLoading)
        {
            control.IsNotLoading = !isLoading;
        }
    }

    private bool IsNotLoading 
    {
        get => (bool)GetValue(IsNotLoadingProperty);
        set => SetValue(IsNotLoadingProperty, value);
    }

    private readonly static DependencyProperty IsNotLoadingProperty =
        DependencyProperty.Register(
            nameof(IsNotLoading),
            typeof(bool),
            typeof(AmbientImageViewer),
            new PropertyMetadata(null));

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private readonly static DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(AmbientImageViewer),
            new PropertyMetadata(null));

    public int ImagesCount
    {
        get => (int)GetValue(ImagesCountProperty);
        set => SetValue(ImagesCountProperty, value);
    }

    private readonly static DependencyProperty ImagesCountProperty =
        DependencyProperty.Register(
            nameof(ImagesCount),
            typeof(int),
            typeof(AmbientImageViewer),
            new PropertyMetadata(null));

    public event EventHandler<object, SelectionChangedEventArgs>? FlipViewSelectionChanged;
    public event EventHandler<object, RoutedEventArgs>? PlayButtonClick;
    public event EventHandler<object, RoutedEventArgs>? OpenButtonClick;

    public void OnPlayButtonClick(object sender, RoutedEventArgs e)
    {
        PlayButtonClick?.Invoke(sender, e);
    }

    public void OnOpenButtonClick(object sender, RoutedEventArgs e)
    {
        OpenButtonClick?.Invoke(sender, e);
    }

    private void OnImageFlipViewLoaded(object sender, RoutedEventArgs e)
    {
        if (imageFlipView.SelectedItem is HeroCarouselItem img)
            glow.SourceImageUrl = img.ImageUrl;
    }

    private void OnFlipViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (imageFlipView.SelectedItem is HeroCarouselItem img)
            glow.SourceImageUrl = img.ImageUrl;

        FlipViewSelectionChanged?.Invoke(sender, e);
    }

    private void MyPipsPager_SelectedIndexChanged(PipsPager sender, PipsPagerSelectedIndexChangedEventArgs args)
    {
        if (_autoScrollTimer != null)
        {
            _autoScrollTimer.Stop();
            _autoScrollTimer.Start();
        }
    }
}
