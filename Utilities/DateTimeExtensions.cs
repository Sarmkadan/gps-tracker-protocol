#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// DateTime extension methods for timestamp handling and formatting.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(dateTime.ToUniversalTime() - epoch).TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    public static DateTime FromUnixTimestamp(long unixTimestamp)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(unixTimestamp);
    }

    /// <summary>
    /// Rounds DateTime down to nearest interval (e.g., 5 minutes).
    /// </summary>
    public static DateTime RoundDown(this DateTime dateTime, TimeSpan interval)
    {
        var ticks = dateTime.Ticks / interval.Ticks;
        return new DateTime(ticks * interval.Ticks, dateTime.Kind);
    }

    /// <summary>
    /// Rounds DateTime up to nearest interval.
    /// </summary>
    public static DateTime RoundUp(this DateTime dateTime, TimeSpan interval)
    {
        var remainder = dateTime.Ticks % interval.Ticks;
        if (remainder == 0)
            return dateTime;

        return dateTime.AddTicks(interval.Ticks - remainder);
    }

    /// <summary>
    /// Checks if DateTime is within past N seconds.
    /// </summary>
    public static bool IsWithinSeconds(this DateTime dateTime, int seconds)
    {
        return (DateTime.UtcNow - dateTime).TotalSeconds <= seconds;
    }

    /// <summary>
    /// Gets human-readable time difference (e.g., "5 minutes ago").
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 60)
            return "just now";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";

        return dateTime.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Gets the start of day in UTC.
    /// </summary>
    public static DateTime GetStartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of day in UTC.
    /// </summary>
    public static DateTime GetEndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of month.
    /// </summary>
    public static DateTime GetStartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of month.
    /// </summary>
    public static DateTime GetEndOfMonth(this DateTime dateTime)
    {
        return dateTime.GetStartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Checks if two DateTimes are on the same day.
    /// </summary>
    public static bool IsSameDay(this DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date;
    }

    /// <summary>
    /// Formats DateTime as ISO 8601 string.
    /// </summary>
    public static string ToIso8601String(this DateTime dateTime)
    {
        return dateTime.ToString("o");
    }
}
