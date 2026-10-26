using Microsoft.UI.Xaml.Media.Imaging;

namespace Nativefy;

public class InverseNullToVisibilityConverter : IValueConverter
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

public class NullToVisibilityConverter : IValueConverter
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

public class SecondsToMinutesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            return time.TotalHours >= 1
                ? time.ToString(@"h\:mm\:ss")
                : time.ToString(@"mm\:ss");
        }
        return "00:00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class ImageSourceFallbackConverter : IValueConverter
{
    private static readonly BitmapImage DefaultArt = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string uriStr && !string.IsNullOrWhiteSpace(uriStr))
        {
            try
            {
                return new BitmapImage(new Uri(uriStr));
            }
            catch
            {
                return DefaultArt;
            }
        }
        return DefaultArt;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class TimeSpanTooltipConverter : IValueConverter
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