using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;

namespace Nativefy.Controls;

/// <summary>
/// YouTube-style Ambient Glow backdrop.
/// Blurs the source image and fades it out smoothly using a 2D rectangular gradient mask.
/// </summary>
public sealed partial class AmbientGlowBackdrop : UserControl
{
    private Compositor? _compositor;
    private ContainerVisual? _containerVisual;

    private CompositionSurfaceBrush?[] _surfaceBrushes = new CompositionSurfaceBrush?[2];
    private SpriteVisual?[] _glowVisuals = new SpriteVisual?[2];
    private Microsoft.UI.Xaml.Media.LoadedImageSurface?[] _imageSurfaces = new Microsoft.UI.Xaml.Media.LoadedImageSurface?[2];

    private int _activeIndex = 0;
    private string? _requestedUrl;
    private string? _loadedUrl;

    private static readonly TimeSpan CrossfadeDuration = TimeSpan.FromMilliseconds(500);

    public AmbientGlowBackdrop()
    {
        InitializeComponent();
        GlowHost.Loaded += OnGlowHostLoaded;
        Unloaded += OnUnloaded;
    }

    #region Dependency Properties

    public static readonly DependencyProperty SourceImageUrlProperty =
        DependencyProperty.Register(nameof(SourceImageUrl), typeof(string), typeof(AmbientGlowBackdrop),
            new PropertyMetadata(null, OnSourceImageUrlChanged));

    public static readonly DependencyProperty IsGlowVisibleProperty =
        DependencyProperty.Register(nameof(IsGlowVisible), typeof(bool), typeof(AmbientGlowBackdrop),
            new PropertyMetadata(true, OnIsGlowVisibleChanged));

    public static readonly DependencyProperty BlurAmountProperty =
        DependencyProperty.Register(nameof(BlurAmount), typeof(double), typeof(AmbientGlowBackdrop),
            new PropertyMetadata(80.0, OnGeometryPropertyChanged));

    public static readonly DependencyProperty GlowScaleProperty =
        DependencyProperty.Register(nameof(GlowScale), typeof(double), typeof(AmbientGlowBackdrop),
            new PropertyMetadata(1.3, OnGeometryPropertyChanged));

    public static readonly DependencyProperty GlowOpacityProperty =
        DependencyProperty.Register(nameof(GlowOpacity), typeof(double), typeof(AmbientGlowBackdrop),
            new PropertyMetadata(0.7));

    public string? SourceImageUrl
    {
        get => (string?)GetValue(SourceImageUrlProperty);
        set => SetValue(SourceImageUrlProperty, value);
    }

    public bool IsGlowVisible
    {
        get => (bool)GetValue(IsGlowVisibleProperty);
        set => SetValue(IsGlowVisibleProperty, value);
    }

    public double BlurAmount
    {
        get => (double)GetValue(BlurAmountProperty);
        set => SetValue(BlurAmountProperty, value);
    }

    public double GlowScale
    {
        get => (double)GetValue(GlowScaleProperty);
        set => SetValue(GlowScaleProperty, value);
    }

    public double GlowOpacity
    {
        get => (double)GetValue(GlowOpacityProperty);
        set => SetValue(GlowOpacityProperty, value);
    }

    #endregion

    #region Public Methods

    public void FadeOut()
    {
        if (_glowVisuals[0] != null) FadeVisualTo(_glowVisuals[0]!, 0f);
        if (_glowVisuals[1] != null) FadeVisualTo(_glowVisuals[1]!, 0f);
    }

    public void FadeIn()
    {
        if (IsGlowVisible && !string.IsNullOrWhiteSpace(_loadedUrl) && _glowVisuals[_activeIndex] != null)
        {
            FadeVisualTo(_glowVisuals[_activeIndex]!, (float)GlowOpacity);
        }
    }

    #endregion

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < 2; i++)
        {
            _imageSurfaces[i]?.Dispose();
            _imageSurfaces[i] = null;

            _surfaceBrushes[i]?.Dispose();
            _surfaceBrushes[i] = null;

            _glowVisuals[i]?.Dispose();
            _glowVisuals[i] = null;
        }

        ElementCompositionPreview.SetElementChildVisual(GlowHost, null);
        _compositor = null;
    }

    private void OnGlowHostLoaded(object sender, RoutedEventArgs e)
    {
        SetupComposition();
        LoadImage(SourceImageUrl);

        GlowHost.LayoutUpdated += OnGlowHostLayoutUpdatedOnce;
    }

    private void OnGlowHostLayoutUpdatedOnce(object? sender, object e)
    {
        if (GlowHost.ActualWidth > 0 && GlowHost.ActualHeight > 0)
        {
            UpdateGlowGeometry(GlowHost.ActualWidth, GlowHost.ActualHeight);
            GlowHost.LayoutUpdated -= OnGlowHostLayoutUpdatedOnce;
        }
    }

    private void SetupComposition()
    {
        var visual = ElementCompositionPreview.GetElementVisual(GlowHost);
        _compositor = visual.Compositor;

        _containerVisual = _compositor.CreateContainerVisual();
        _containerVisual.RelativeSizeAdjustment = Vector2.One;

        for (int i = 0; i < 2; i++)
        {
            // 1. Raw Image Surface Brush
            var surfaceBrush = _compositor.CreateSurfaceBrush();
            surfaceBrush.Stretch = CompositionStretch.Fill;

            // 2. Horizontal Linear Gradient Mask (fades left and right edges)
            var horizGradient = _compositor.CreateLinearGradientBrush();
            horizGradient.StartPoint = new Vector2(0f, 0.5f);
            horizGradient.EndPoint = new Vector2(1f, 0.5f);
            horizGradient.ColorStops.Add(_compositor.CreateColorGradientStop(0.0f, Windows.UI.Color.FromArgb(0, 255, 255, 255)));
            horizGradient.ColorStops.Add(_compositor.CreateColorGradientStop(0.2f, Windows.UI.Color.FromArgb(255, 255, 255, 255)));
            horizGradient.ColorStops.Add(_compositor.CreateColorGradientStop(0.8f, Windows.UI.Color.FromArgb(255, 255, 255, 255)));
            horizGradient.ColorStops.Add(_compositor.CreateColorGradientStop(1.0f, Windows.UI.Color.FromArgb(0, 255, 255, 255)));

            // 3. Vertical Linear Gradient Mask (fades top and bottom edges)
            var vertGradient = _compositor.CreateLinearGradientBrush();
            vertGradient.StartPoint = new Vector2(0.5f, 0f);
            vertGradient.EndPoint = new Vector2(0.5f, 1f);
            vertGradient.ColorStops.Add(_compositor.CreateColorGradientStop(0.0f, Windows.UI.Color.FromArgb(0, 255, 255, 255)));
            vertGradient.ColorStops.Add(_compositor.CreateColorGradientStop(0.2f, Windows.UI.Color.FromArgb(255, 255, 255, 255)));
            vertGradient.ColorStops.Add(_compositor.CreateColorGradientStop(0.8f, Windows.UI.Color.FromArgb(255, 255, 255, 255)));
            vertGradient.ColorStops.Add(_compositor.CreateColorGradientStop(1.0f, Windows.UI.Color.FromArgb(0, 255, 255, 255)));

            // 4. Combine horizontal & vertical gradients into a 2D rectangular mask via ArithmeticComposite (Multiply)
            var rectMaskEffect = new ArithmeticCompositeEffect
            {
                Name = "RectMask",
                MultiplyAmount = 1f,
                Source1Amount = 0f,
                Source2Amount = 0f,
                Offset = 0f,
                Source1 = new CompositionEffectSourceParameter("HorizMask"),
                Source2 = new CompositionEffectSourceParameter("VertMask")
            };

            var maskEffectFactory = _compositor.CreateEffectFactory(rectMaskEffect);
            var rectMaskBrush = maskEffectFactory.CreateBrush();
            rectMaskBrush.SetSourceParameter("HorizMask", horizGradient);
            rectMaskBrush.SetSourceParameter("VertMask", vertGradient);

            // 5. Gaussian Blur Effect on the Image
            var blurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = (float)BlurAmount,
                BorderMode = EffectBorderMode.Hard,
                Source = new CompositionEffectSourceParameter("Source")
            };

            var blurFactory = _compositor.CreateEffectFactory(blurEffect);
            var blurBrush = blurFactory.CreateBrush();
            blurBrush.SetSourceParameter("Source", surfaceBrush);

            // 6. AlphaMaskEffect to apply our 2D rectangular gradient mask onto the blurred image
            var finalGlowEffect = new AlphaMaskEffect
            {
                Source = new CompositionEffectSourceParameter("ImageSource"),
                AlphaMask = new CompositionEffectSourceParameter("MaskSource")
            };

            var finalFactory = _compositor.CreateEffectFactory(finalGlowEffect);
            var finalBrush = finalFactory.CreateBrush();
            finalBrush.SetSourceParameter("ImageSource", blurBrush);
            finalBrush.SetSourceParameter("MaskSource", rectMaskBrush);

            // 7. Glow SpriteVisual
            var glowVisual = _compositor.CreateSpriteVisual();
            glowVisual.Brush = finalBrush;
            glowVisual.RelativeSizeAdjustment = Vector2.One;
            glowVisual.Opacity = 0f;

            _containerVisual.Children.InsertAtTop(glowVisual);

            _surfaceBrushes[i] = surfaceBrush;
            _glowVisuals[i] = glowVisual;
        }

        UpdateGlowGeometry(GlowHost.ActualWidth, GlowHost.ActualHeight);

        ElementCompositionPreview.SetElementChildVisual(GlowHost, _containerVisual);

        GlowHost.SizeChanged -= OnGlowHostSizeChanged;
        GlowHost.SizeChanged += OnGlowHostSizeChanged;
    }

    private void OnGlowHostSizeChanged(object sender, SizeChangedEventArgs args)
    {
        UpdateGlowGeometry(args.NewSize.Width, args.NewSize.Height);
    }

    private void UpdateGlowGeometry(double width, double height)
    {
        if (width <= 0 || height <= 0 || _compositor == null) return;

        var center = new Vector3((float)(width / 2.0), (float)(height / 2.0), 0);

        for (int i = 0; i < 2; i++)
        {
            if (_glowVisuals[i] != null)
            {
                _glowVisuals[i]!.CenterPoint = center;
                _glowVisuals[i]!.Scale = new Vector3((float)GlowScale);
            }
        }
    }

    private void LoadImage(string? url)
    {
        if (_compositor == null || _glowVisuals[0] == null || _glowVisuals[1] == null) return;

        _requestedUrl = url;

        if (string.IsNullOrWhiteSpace(url))
        {
            _loadedUrl = null;
            FadeOut();
            return;
        }

        if (string.Equals(url, _loadedUrl, StringComparison.OrdinalIgnoreCase))
            return;

        var nextIndex = 1 - _activeIndex;
        var nextBrush = _surfaceBrushes[nextIndex]!;
        var nextVisual = _glowVisuals[nextIndex]!;

        _imageSurfaces[nextIndex]?.Dispose();

        var desiredSize = new Windows.Foundation.Size(
            Math.Max(1, GlowHost.ActualWidth > 0 ? GlowHost.ActualWidth / 2 : 400),
            Math.Max(1, GlowHost.ActualHeight > 0 ? GlowHost.ActualHeight / 2 : 225));

        var newSurface = Microsoft.UI.Xaml.Media.LoadedImageSurface.StartLoadFromUri(new Uri(url), desiredSize);
        _imageSurfaces[nextIndex] = newSurface;
        nextBrush.Surface = newSurface;
        _loadedUrl = url;

        newSurface.LoadCompleted += (_, args) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (!ReferenceEquals(newSurface, _imageSurfaces[nextIndex])
                    || !string.Equals(url, _requestedUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (args.Status != LoadedImageSourceLoadStatus.Success)
                {
                    FadeOut();
                    return;
                }

                var previousIndex = _activeIndex;
                _activeIndex = nextIndex;

                if (IsGlowVisible)
                {
                    FadeVisualTo(nextVisual, (float)GlowOpacity);
                }

                if (previousIndex != nextIndex && _glowVisuals[previousIndex] != null)
                {
                    FadeVisualTo(_glowVisuals[previousIndex]!, 0f);
                }
            });
        };
    }

    private void FadeVisualTo(SpriteVisual target, float opacity)
    {
        if (_compositor == null)
        {
            target.Opacity = opacity;
            return;
        }

        var anim = _compositor.CreateScalarKeyFrameAnimation();
        anim.InsertKeyFrame(1f, opacity,
            _compositor.CreateCubicBezierEasingFunction(new Vector2(0.25f, 0.1f), new Vector2(0.25f, 1f)));
        anim.Duration = CrossfadeDuration;
        target.StartAnimation("Opacity", anim);
    }

    private static void OnSourceImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AmbientGlowBackdrop control)
            control.LoadImage(e.NewValue as string);
    }

    private static void OnIsGlowVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AmbientGlowBackdrop control)
        {
            if ((bool)e.NewValue)
                control.FadeIn();
            else
                control.FadeOut();
        }
    }

    private static void OnGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AmbientGlowBackdrop control)
            control.UpdateGlowGeometry(control.GlowHost.ActualWidth, control.GlowHost.ActualHeight);
    }
}