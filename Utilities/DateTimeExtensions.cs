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
    /// <remarks>
    /// Values that are not <see cref="DateTimeKind.Utc"/> are converted with <see cref="DateTime.ToUniversalTime"/>,
    /// so <see cref="DateTimeKind.Unspecified"/> values are interpreted as local time.
    /// Fractional seconds are truncated.
    /// </remarks>
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
    /// Converts DateTime to Unix timestamp (milliseconds since epoch).
    /// </summary>
    /// <remarks>
    /// Values that are not <see cref="DateTimeKind.Utc"/> are converted with <see cref="DateTime.ToUniversalTime"/>,
    /// so <see cref="DateTimeKind.Unspecified"/> values are interpreted as local time.
    /// Fractional milliseconds are truncated.
    /// </remarks>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>Unix timestamp in milliseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the date is before Unix epoch.</exception>
    public static long ToUnixMillis(this DateTime dateTime)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();

        if (utcDateTime < epoch)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime cannot be before Unix epoch (1970-01-01).");
        }

        return (utcDateTime - epoch).Ticks / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    /// <remarks>
    /// The result has <see cref="DateTimeKind.Utc"/> kind.
    /// </remarks>
    /// <param name="unixTimestamp">Unix timestamp in seconds.</param>
    /// <returns>UTC DateTime representing the Unix timestamp.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timestamp is negative or too large to represent as a DateTime.</exception>
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
    /// <remarks>
    /// Alignment is based on ticks since 0001-01-01, so the result keeps the original <see cref="DateTimeKind"/>.
    /// </remarks>
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
    /// <remarks>
    /// Alignment is based on ticks since 0001-01-01, so the result keeps the original <see cref="DateTimeKind"/>.
    /// A value already on an interval boundary is returned unchanged.
    /// </remarks>
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
    /// <remarks>
    /// Compares against <see cref="DateTime.UtcNow"/>. Future values are also reported as within range,
    /// because the elapsed time is negative. <see cref="DateTimeKind.Unspecified"/> values are interpreted as local time.
    /// </remarks>
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
    /// <remarks>
    /// Compares against <see cref="DateTime.UtcNow"/>. Under one minute, including future values, returns "just now".
    /// Values seven days or older are returned as a UTC date in <c>yyyy-MM-dd</c> format.
    /// </remarks>
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
            _ => utcDateTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Gets the start of the day (midnight).
    /// </summary>
    /// <remarks>
    /// No time zone conversion is performed. The result keeps the original <see cref="DateTimeKind"/>.
    /// </remarks>
    /// <param name="dateTime">The DateTime to get start of day for.</param>
    /// <returns>DateTime representing the start of the day (midnight).</returns>
    public static DateTime GetStartOfDay(this DateTime dateTime) => dateTime.Date;

    /// <summary>
    /// Gets the end of the day (last tick before the next midnight).
    /// </summary>
    /// <remarks>
    /// No time zone conversion is performed. The result keeps the original <see cref="DateTimeKind"/>.
    /// </remarks>
    /// <param name="dateTime">The DateTime to get end of day for.</param>
    /// <returns>DateTime representing the end of the day (23:59:59.9999999).</returns>
    public static DateTime GetEndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);

    /// <summary>
    /// Gets the start of month.
    /// </summary>
    /// <remarks>
    /// The result has <see cref="DateTimeKind.Unspecified"/> kind; the original kind is not preserved.
    /// </remarks>
    /// <param name="dateTime">The DateTime to get start of month for.</param>
    /// <returns>DateTime representing the first day of the month at midnight.</returns>
    public static DateTime GetStartOfMonth(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, 1);

    /// <summary>
    /// Gets the end of month.
    /// </summary>
    /// <remarks>
    /// The result has <see cref="DateTimeKind.Unspecified"/> kind; the original kind is not preserved.
    /// </remarks>
    /// <param name="dateTime">The DateTime to get end of month for.</param>
    /// <returns>DateTime representing the last tick of the last day of the month.</returns>
    public static DateTime GetEndOfMonth(this DateTime dateTime) => dateTime.GetStartOfMonth().AddMonths(1).AddTicks(-1);

    /// <summary>
    /// Checks if two DateTimes are on the same day.
    /// </summary>
    /// <remarks>
    /// Only the date portions are compared. No time zone conversion is performed, and <see cref="DateTimeKind"/> is ignored.
    /// </remarks>
    /// <param name="date1">First DateTime to compare.</param>
    /// <param name="date2">Second DateTime to compare.</param>
    /// <returns>True if both dates are on the same day; otherwise false.</returns>
    public static bool IsSameDay(this DateTime date1, DateTime date2) => date1.Date == date2.Date;

    /// <summary>
    /// Formats DateTime as ISO 8601 string.
    /// </summary>
    /// <remarks>
    /// Uses the round-trip <c>"o"</c> format. The output ends with <c>Z</c> for UTC values and includes an offset for local values.
    /// </remarks>
    /// <param name="dateTime">The DateTime to format.</param>
    /// <returns>ISO 8601 formatted string.</returns>
    public static string ToIso8601String(this DateTime dateTime) => dateTime.ToString("o");
}
