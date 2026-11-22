using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace Nativefy;

public sealed partial class ImageUrlFallbackConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        Debug.WriteLine($"[Image Binding] {value ?? "<null>"}");

        if (value is string url && Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            return new BitmapImage(uri);

        return new BitmapImage(new Uri("ms-appx:///Assets/image.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public partial class InverseNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Visible if value is NOT null (and not an empty string, if it's a string)
        if (value is string s)
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;

        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string s)
            return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;

        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public sealed partial class TimeSpanTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var ms = System.Convert.ToDouble(value);
        var ts = TimeSpan.FromMilliseconds(ms);
        return ts.Hours > 0 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed partial class BoolToStringConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isLiked = System.Convert.ToBoolean(value);
        return isLiked ? "Unlike" : "Like";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}