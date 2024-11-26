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
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>Unix timestamp in seconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the date is before Unix epoch.</exception>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();

        if (utcDateTime < epoch)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime cannot be before Unix epoch (1970-01-01).");
        }

        return (long)(utcDateTime - epoch).TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    /// <param name="unixTimestamp">Unix timestamp in seconds.</param>
    /// <returns>DateTime representing the Unix timestamp.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timestamp is negative.</exception>
    public static DateTime FromUnixTimestamp(long unixTimestamp)
    {
        if (unixTimestamp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unixTimestamp), "Unix timestamp cannot be negative.");
        }

        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(unixTimestamp);
    }

    /// <summary>
    /// Rounds DateTime down to nearest interval (e.g., 5 minutes).
    /// </summary>
    /// <param name="dateTime">The DateTime to round down.</param>
    /// <param name="interval">The rounding interval.</param>
    /// <returns>DateTime rounded down to the nearest interval.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when interval is zero or negative.</exception>
    public static DateTime RoundDown(this DateTime dateTime, TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

        var ticks = dateTime.Ticks / interval.Ticks;
        return new DateTime(ticks * interval.Ticks, dateTime.Kind);
    }

    /// <summary>
    /// Rounds DateTime up to nearest interval.
    /// </summary>
    /// <param name="dateTime">The DateTime to round up.</param>
    /// <param name="interval">The rounding interval.</param>
    /// <returns>DateTime rounded up to the nearest interval.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when interval is zero or negative.</exception>
    public static DateTime RoundUp(this DateTime dateTime, TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

        var remainder = dateTime.Ticks % interval.Ticks;
        return remainder == 0
            ? dateTime
            : dateTime.AddTicks(interval.Ticks - remainder);
    }

    /// <summary>
    /// Checks if DateTime is within past N seconds.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <param name="seconds">Number of seconds to check within.</param>
    /// <returns>True if the DateTime is within the specified seconds from now; otherwise false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when seconds is negative.</exception>
    public static bool IsWithinSeconds(this DateTime dateTime, int seconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seconds);

        var utcNow = DateTime.UtcNow;
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();

        return (utcNow - utcDateTime).TotalSeconds <= seconds;
    }

    /// <summary>
    /// Gets human-readable time difference (e.g., "5 minutes ago").
    /// </summary>
    /// <param name="dateTime">The DateTime to convert to relative time.</param>
    /// <returns>Human-readable relative time string.</returns>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var utcNow = DateTime.UtcNow;
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();

        var timeSpan = utcNow - utcDateTime;

        return timeSpan.TotalSeconds switch
        {
            < 60 => "just now",
            < 3600 => $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago",
            < 86400 => $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago",
            < 604800 => $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago",
            _ => utcDateTime.ToString("yyyy-MM-dd")
        };
    }

    /// <summary>
    /// Gets the start of day in UTC.
    /// </summary>
    /// <param name="dateTime">The DateTime to get start of day for.</param>
    /// <returns>DateTime representing the start of the day (midnight).</returns>
    public static DateTime GetStartOfDay(this DateTime dateTime) => dateTime.Date;

    /// <summary>
    /// Gets the end of day in UTC.
    /// </summary>
    /// <param name="dateTime">The DateTime to get end of day for.</param>
    /// <returns>DateTime representing the end of the day (23:59:59.9999999).</returns>
    public static DateTime GetEndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);

    /// <summary>
    /// Gets the start of month.
    /// </summary>
    /// <param name="dateTime">The DateTime to get start of month for.</param>
    /// <returns>DateTime representing the first day of the month at midnight.</returns>
    public static DateTime GetStartOfMonth(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, 1);

    /// <summary>
    /// Gets the end of month.
    /// </summary>
    /// <param name="dateTime">The DateTime to get end of month for.</param>
    /// <returns>DateTime representing the last tick of the last day of the month.</returns>
    public static DateTime GetEndOfMonth(this DateTime dateTime) => dateTime.GetStartOfMonth().AddMonths(1).AddTicks(-1);

    /// <summary>
    /// Checks if two DateTimes are on the same day.
    /// </summary>
    /// <param name="date1">First DateTime to compare.</param>
    /// <param name="date2">Second DateTime to compare.</param>
    /// <returns>True if both dates are on the same day; otherwise false.</returns>
    public static bool IsSameDay(this DateTime date1, DateTime date2) => date1.Date == date2.Date;

    /// <summary>
    /// Formats DateTime as ISO 8601 string.
    /// </summary>
    /// <param name="dateTime">The DateTime to format.</param>
    /// <returns>ISO 8601 formatted string.</returns>
    public static string ToIso8601String(this DateTime dateTime) => dateTime.ToString("o");
}
