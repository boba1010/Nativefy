using Nativefy.Models.Media;

namespace Nativefy.Helpers;

public static partial class DurationHelper
{
    public static string FormatDuration(TimeSpan? duration)
    {
        if (!duration.HasValue)
            return "";

        var value = duration.Value;

        return value.TotalHours >= 1 ? $"{(int)value.TotalHours:D2}:{value.Minutes:D2}:{value.Seconds:D2}" : value.ToString(@"mm\:ss");
    }

    public static string FormatDuration(TimeSpan duration) =>
        duration.TotalHours >= 1 ? $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}" : duration.ToString(@"m\:ss");

    public static string FormatTotalDuration(List<MediaItem> items)
    {
        var total = TimeSpan.Zero;

        foreach (var item in items)
            total += item.Duration;

        var hours = (int)total.TotalHours;
        var minutes = total.Minutes;

        if (hours >= 1)
            return $"{hours} hr {minutes} min";

        return $"{minutes} min";
    }
}
