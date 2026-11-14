using System.Globalization;

namespace Nativefy.Helpers;

public static class DatetimeHelper
{
    public static string FormatToEnglish(DateTime dateTime)
    {
        DateTime now = DateTime.Now;
        TimeSpan difference = now - dateTime;

        if (difference.TotalMinutes < 1)
            return "Just now";

        if (difference.TotalHours < 1)
            return $"{(int)difference.TotalMinutes} minutes ago";

        if (difference.TotalDays < 1)
            return $"{(int)difference.TotalHours} hours ago";

        if (difference.TotalDays < 2)
            return "Yesterday";

        if (difference.TotalDays < 7)
            return $"{(int)difference.TotalDays} days ago";

        return dateTime.ToString("MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));
    }
}