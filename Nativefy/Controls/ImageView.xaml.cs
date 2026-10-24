using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using System.Numerics;

namespace Nativefy.Controls;

public sealed partial class ImageView : UserControl
{
    private CompositionSurfaceBrush? _surfaceBrush;
    private SpriteVisual? _imageVisual;

    private Compositor? _compositor;
    private LoadedImageSurface? _imageSurface;
    private CompositionRectangleGeometry? _clipGeometry; // Added clip geometry reference
    private bool _hasAnimated;
    private bool _animateNextImageLoad = true;
    private string? _loadedImageUrl;
    private string? _requestedImageUrl;

    public ImageView()
    {
        InitializeComponent();
        ImageBorder.Loaded += OnImageBorderLoaded;
        Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty ImageUrlProperty =
        DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(ImageView),
            new PropertyMetadata(null, OnImageUrlChanged));

    public static readonly DependencyProperty OverlayContentProperty =
        DependencyProperty.Register(nameof(OverlayContent), typeof(object), typeof(ImageView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FallbackTextProperty =
        DependencyProperty.Register(nameof(FallbackText), typeof(string), typeof(ImageView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InitialScaleProperty =
        DependencyProperty.Register(nameof(InitialScale), typeof(double), typeof(ImageView),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty FinalScaleProperty =
        DependencyProperty.Register(nameof(FinalScale), typeof(double), typeof(ImageView),
            new PropertyMetadata(1.05));

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(ImageView),
            new PropertyMetadata(TimeSpan.FromMilliseconds(800)));

    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public object? OverlayContent
    {
        get => GetValue(OverlayContentProperty);
        set => SetValue(OverlayContentProperty, value);
    }

    /// <summary>Text shown in the gray-gradient fallback state when ImageUrl is null/empty.</summary>
    public string? FallbackText
    {
        get => (string?)GetValue(FallbackTextProperty);
        set => SetValue(FallbackTextProperty, value);
    }

    public double InitialScale
    {
        get => (double)GetValue(InitialScaleProperty);
        set => SetValue(InitialScaleProperty, value);
    }

    public double FinalScale
    {
        get => (double)GetValue(FinalScaleProperty);
        set => SetValue(FinalScaleProperty, value);
    }

    public TimeSpan AnimationDuration
    {
        get => (TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _imageSurface?.Dispose();
        _imageSurface = null;

        _clipGeometry = null;

        if (_surfaceBrush != null)
        {
            _surfaceBrush.Surface = null;
            _surfaceBrush.Dispose();
            _surfaceBrush = null;
        }

        if (_imageVisual != null)
        {
            ElementCompositionPreview.SetElementChildVisual(ImageBorder, null);
            _imageVisual.Dispose();
            _imageVisual = null;
        }

        _compositor = null;
    }

    private void OnImageBorderLoaded(object sender, RoutedEventArgs e)
    {
        SetupComposition();
        LoadImage(ImageUrl);
    }

    private void SetupComposition()
    {
        var visual = ElementCompositionPreview.GetElementVisual(ImageBorder);
        _compositor = visual.Compositor;

        _surfaceBrush = _compositor.CreateSurfaceBrush();
        _surfaceBrush.Stretch = CompositionStretch.UniformToFill;
        _surfaceBrush.VerticalAlignmentRatio = 0.5f;

        _imageVisual = _compositor.CreateSpriteVisual();
        _imageVisual.Brush = _surfaceBrush;
        _imageVisual.RelativeSizeAdjustment = Vector2.One;

        // --- CLIP FIX ---
        // Prevents 1.05x scaled image from overflowing past ImageBorder bounds
        _clipGeometry = _compositor.CreateRectangleGeometry();
        _clipGeometry.Size = new Vector2((float)ImageBorder.ActualWidth, (float)ImageBorder.ActualHeight);
        _imageVisual.Clip = _compositor.CreateGeometricClip(_clipGeometry);
        // ----------------

        _imageVisual.CenterPoint = new Vector3(
            (float)(ImageBorder.ActualWidth / 2),
            (float)(ImageBorder.ActualHeight / 2), 0);

        if (_hasAnimated)
        {
            _imageVisual.Scale = new Vector3((float)FinalScale);
            _imageVisual.Opacity = 1f;
        }
        else
        {
            _imageVisual.Scale = new Vector3((float)InitialScale);
            _imageVisual.Opacity = 0f;
        }

        ElementCompositionPreview.SetElementChildVisual(ImageBorder, _imageVisual);

        ImageBorder.SizeChanged -= OnImageBorderSizeChanged;
        ImageBorder.SizeChanged += OnImageBorderSizeChanged;
    }

    private void LoadImage(string? url)
    {
        if (_surfaceBrush == null || _compositor == null) return;

        var normalizedUrl = NormalizeImageUrl(url);
        _requestedImageUrl = normalizedUrl;
        _animateNextImageLoad = !string.Equals(_loadedImageUrl, normalizedUrl, StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(normalizedUrl))
        {
            _loadedImageUrl = null;
            _hasAnimated = false;
            _surfaceBrush.Surface = null;

            if (_imageVisual != null)
            {
                _imageVisual.StopAnimation("Scale");
                _imageVisual.StopAnimation("Opacity");
                _imageVisual.Scale = new Vector3((float)InitialScale);
                _imageVisual.Opacity = 0f;
            }
            return;
        }

        var shouldAnimate = _animateNextImageLoad;
        PrepareForImageLoad(shouldAnimate);

        _imageSurface?.Dispose();
        var desiredSize = new Windows.Foundation.Size(
            Math.Max(1, ImageBorder.ActualWidth > 0 ? ImageBorder.ActualWidth : 1200),
            Math.Max(1, ImageBorder.ActualHeight > 0 ? ImageBorder.ActualHeight : 420));
        _imageSurface = Microsoft.UI.Xaml.Media.LoadedImageSurface.StartLoadFromUri(new Uri(normalizedUrl), desiredSize);

        _surfaceBrush.Surface = _imageSurface;
        _loadedImageUrl = normalizedUrl;

        var surface = _imageSurface;
        surface.LoadCompleted += (_, args) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (!ReferenceEquals(surface, _imageSurface)
                    || !string.Equals(normalizedUrl, _requestedImageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (args.Status == LoadedImageSourceLoadStatus.Success)
                {
                    PlayPopInAnimation();
                }
                else if (_imageVisual != null)
                {
                    _imageVisual.Opacity = 1f;
                    _imageVisual.Scale = new Vector3((float)FinalScale);
                }
            });
        };
    }

    private void PrepareForImageLoad(bool shouldAnimate)
    {
        if (_imageVisual == null) return;

        _imageVisual.StopAnimation("Scale");
        _imageVisual.StopAnimation("Opacity");

        if (shouldAnimate)
        {
            _hasAnimated = false;
            _imageVisual.Scale = new Vector3((float)InitialScale);
            _imageVisual.Opacity = 0f;
        }
        else
        {
            _hasAnimated = true;
            _imageVisual.Scale = new Vector3((float)FinalScale);
            _imageVisual.Opacity = 1f;
        }
    }

    private void OnImageBorderSizeChanged(object sender, SizeChangedEventArgs args)
    {
        if (_imageVisual != null)
        {
            _imageVisual.CenterPoint = new Vector3(
                (float)(args.NewSize.Width / 2),
                (float)(args.NewSize.Height / 2), 0);
        }

        // Keep the clip size perfectly synced with ImageBorder size
        if (_clipGeometry != null)
        {
            _clipGeometry.Size = new Vector2((float)args.NewSize.Width, (float)args.NewSize.Height);
        }
    }

    private void PlayPopInAnimation()
    {
        if (_imageVisual == null || _compositor == null) return;

        if (_hasAnimated)
        {
            _imageVisual.Opacity = 1f;
            _imageVisual.Scale = new Vector3((float)FinalScale);
            return;
        }
        _hasAnimated = true;

        var duration = AnimationDuration;
        var easing = _compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1f));

        var scaleAnim = _compositor.CreateVector3KeyFrameAnimation();
        scaleAnim.InsertKeyFrame(0f, new Vector3((float)InitialScale));
        scaleAnim.InsertKeyFrame(1f, new Vector3((float)FinalScale), easing);
        scaleAnim.Duration = duration;

        var opacityAnim = _compositor.CreateScalarKeyFrameAnimation();
        opacityAnim.InsertKeyFrame(0f, 0f);
        opacityAnim.InsertKeyFrame(0.4f, 1f, easing);
        opacityAnim.Duration = duration;

        _imageVisual.StartAnimation("Scale", scaleAnim);
        _imageVisual.StartAnimation("Opacity", opacityAnim);
    }

    private static string? NormalizeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        return url;
    }

    private static void OnImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageView header)
            header.LoadImage(e.NewValue as string);
    }
}