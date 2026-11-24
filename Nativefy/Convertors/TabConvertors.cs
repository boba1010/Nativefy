namespace Nativefy.Convertors;

public sealed partial class VideosTabConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var tab = (Tab)value;

        return tab == Tab.Videos ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public sealed partial class HomeTabConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var tab = (Tab)value;

        return tab == Tab.Home ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
