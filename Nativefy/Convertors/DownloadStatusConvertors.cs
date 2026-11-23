using Microsoft.UI;
using Nativefy.ViewModels.Shell;

namespace Nativefy.Convertors;

public sealed partial class DownloadStatusToBrushConvertor : IValueConverter
{
    private static readonly Brush FailedBrush = new SolidColorBrush(Colors.Red);
    private static readonly Brush CanceledBrush = new SolidColorBrush(Colors.Yellow);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var downloadStatus = (DownloadStatus)value;

        return downloadStatus switch
        {
            DownloadStatus.Failed => FailedBrush,
            DownloadStatus.Canceled => CanceledBrush,
            _ => (Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"]
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public sealed partial class DownloadStatusToVisiblityConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var downloadStatus = (DownloadStatus)value;

        if (downloadStatus is DownloadStatus.Downloading or DownloadStatus.Canceled or DownloadStatus.Failed)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public sealed partial class InverseDownloadStatusToVisiblityConvertor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var downloadStatus = (DownloadStatus)value;

        return downloadStatus == DownloadStatus.Downloaded ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}