using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition;

namespace Nativefy.Brushes;

public class BlurredImageBrush : XamlCompositionBrushBase
{
    public static readonly DependencyProperty ImageUrlProperty =
        DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(BlurredImageBrush),
            new PropertyMetadata(null, OnSourceChanged));

    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public static readonly DependencyProperty BlurAmountProperty =
        DependencyProperty.Register(nameof(BlurAmount), typeof(double), typeof(BlurredImageBrush),
            new PropertyMetadata(20.0, OnBlurChanged));

    public double BlurAmount
    {
        get => (double)GetValue(BlurAmountProperty);
        set => SetValue(BlurAmountProperty, value);
    }

    private LoadedImageSurface? _surface;
    private CompositionEffectBrush? _effectBrush;
    private CompositionSurfaceBrush? _surfaceBrush;

    protected override void OnConnected()
    {
        if (CompositionBrush == null && !string.IsNullOrWhiteSpace(ImageUrl))
            LoadImage();
    }

    protected override void OnDisconnected()
    {
        DisposeResources();
    }

    private void DisposeResources()
    {
        CompositionBrush = null;
        _effectBrush?.Dispose();
        _effectBrush = null;
        _surfaceBrush?.Dispose();
        _surfaceBrush = null;
        if (_surface != null)
        {
            _surface.LoadCompleted -= OnLoadCompleted;
            _surface.Dispose();
            _surface = null;
        }
    }

    private void LoadImage()
    {
        var uri = ResolveUri(ImageUrl);
        if (uri == null)
        {
            DisposeResources();
            return;
        }

        // Clean up any previous surface/brush before creating new ones
        DisposeResources();

        var compositor = Window.Current?.Compositor
            ?? CompositionTarget.GetCompositorForCurrentThread();

        _surface = LoadedImageSurface.StartLoadFromUri(uri);
        _surface.LoadCompleted += OnLoadCompleted;

        _surfaceBrush = compositor.CreateSurfaceBrush(_surface);
        _surfaceBrush.Stretch = CompositionStretch.UniformToFill;

        var blur = new GaussianBlurEffect
        {
            Name = "Blur",
            BlurAmount = (float)BlurAmount,
            BorderMode = EffectBorderMode.Hard,
            Source = new CompositionEffectSourceParameter("source")
        };

        var factory = compositor.CreateEffectFactory(blur, new[] { "Blur.BlurAmount" });
        _effectBrush = factory.CreateBrush();
        _effectBrush.SetSourceParameter("source", _surfaceBrush);

        CompositionBrush = _effectBrush;
    }

    private void OnLoadCompleted(LoadedImageSurface sender, LoadedImageSourceLoadCompletedEventArgs args)
    {
        if (args.Status != LoadedImageSourceLoadStatus.Success)
        {
            // Image failed to load (bad URL, 404, no network, etc.) — fall back to nothing/transparent
             ImageUrl = "ms-appx:///Assets/DefaultAlbumArt.png";
            DisposeResources();
        }
    }

    /// <summary>
    /// Accepts absolute http(s)/ms-appx/file URIs as-is. A bare relative path
    /// (e.g. "Assets/cover.png") is assumed to live in the app package.
    /// Returns null for null/empty/unparsable input.
    /// </summary>
    private static Uri? ResolveUri(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
            return absolute;

        // Relative path fallback — treat as app package asset
        var normalized = path.TrimStart('/', '\\');
        return Uri.TryCreate($"ms-appx:///{normalized}", UriKind.Absolute, out var appxUri)
            ? appxUri
            : null;
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var b = (BlurredImageBrush)d;
        if (b.CompositionBrush != null || (string?)e.NewValue is not null)
            b.LoadImage();
    }

    private static void OnBlurChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var b = (BlurredImageBrush)d;
        b._effectBrush?.Properties.InsertScalar("Blur.BlurAmount", (float)(double)e.NewValue);
    }
}
