#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// String utility extensions for parsing and formatting.
/// Used for NMEA sentence parsing and device ID manipulation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Safely parses string to double with fallback value.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="defaultValue">The value to return if parsing fails.</param>
    /// <returns>The parsed double value or the default value if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static double ToDoubleOrDefault(this string value, double defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return double.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Safely parses string to int with fallback value.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="defaultValue">The value to return if parsing fails.</param>
    /// <returns>The parsed int value or the default value if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static int ToIntOrDefault(this string value, int defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Splits NMEA sentence by comma and returns fields with trimming.
    /// </summary>
    /// <param name="sentence">The NMEA sentence to split.</param>
    /// <returns>Array of trimmed fields.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sentence"/> is null.</exception>
    public static string[] SplitNmea(this string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);

        if (string.IsNullOrEmpty(sentence))
            return [];

        return sentence.Split(',').Select(s => s.Trim()).ToArray();
    }

    /// <summary>
    /// Extracts NMEA checksum from sentence (after * character).
    /// </summary>
    /// <param name="sentence">The NMEA sentence containing a checksum.</param>
    /// <returns>The checksum part after * character, or empty string if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sentence"/> is null.</exception>
    public static string GetNmeaChecksum(this string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);

        var parts = sentence.Split('*');
        return parts.Length == 2 ? parts[1] : string.Empty;
    }

    /// <summary>
    /// Removes NMEA checksum from sentence.
    /// </summary>
    /// <param name="sentence">The NMEA sentence with checksum.</param>
    /// <returns>The sentence without the checksum part.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sentence"/> is null.</exception>
    public static string RemoveNmeaChecksum(this string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);

        var parts = sentence.Split('*');
        return parts[0];
    }

    /// <summary>
    /// Validates IMEI format (15-20 digits).
    /// </summary>
    /// <param name="imei">The IMEI string to validate.</param>
    /// <returns>True if valid IMEI format, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="imei"/> is null.</exception>
    public static bool IsValidImei(this string imei)
    {
        ArgumentNullException.ThrowIfNull(imei);

        if (string.IsNullOrWhiteSpace(imei))
            return false;

        return imei.Length is >= 15 and <= 20 && imei.All(char.IsDigit);
    }

    /// <summary>
    /// Validates device ID format (non-empty, alphanumeric with dashes and underscores).
    /// </summary>
    /// <param name="deviceId">The device ID to validate.</param>
    /// <returns>True if valid device ID format, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static bool IsValidDeviceId(this string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return false;

        return deviceId.Length <= 50 && deviceId.All(c => char.IsLetterOrDigit(c) || c is '-' or '_');
    }

    /// <summary>
    /// Truncates string to maximum length with ellipsis.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">Maximum length of the result string.</param>
    /// <param name="suffix">Suffix to append when truncating (default: "...").</param>
    /// <returns>Truncated string with suffix if needed, or original string if within max length.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (value.Length <= maxLength)
            return value;

        return value[..Math.Max(0, maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts hex string to byte array.
    /// </summary>
    /// <param name="hexString">The hexadecimal string to convert.</param>
    /// <returns>Byte array representation, or empty array if conversion fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="hexString"/> is null.</exception>
    public static byte[] HexToByteArray(this string hexString)
    {
        ArgumentNullException.ThrowIfNull(hexString);

        if (string.IsNullOrWhiteSpace(hexString))
            return [];

        hexString = hexString.Replace("-", "").Replace(" ", "");

        if (hexString.Length % 2 != 0)
            return [];

        try
        {
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Sanitizes device ID by removing/replacing invalid characters.
    /// </summary>
    /// <param name="deviceId">The device ID to sanitize.</param>
    /// <returns>Sanitized device ID, or "unknown" if result is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static string SanitizeDeviceId(this string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return "unknown";

        var sanitized = new string(deviceId
            .Where(c => char.IsLetterOrDigit(c) || c is '-' or '_')
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized[..Math.Min(50, sanitized.Length)];
    }

    /// <summary>
    /// Checks if string is a valid hex color code.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if valid hex color format, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValidHexColor(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.TrimStart('#');
        return (value.Length is 6 or 8) && value.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }
}