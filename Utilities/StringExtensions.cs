#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static double ToDoubleOrDefault(this string value, double defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return double.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Safely parses string to int with fallback value.
    /// </summary>
    public static int ToIntOrDefault(this string value, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Splits NMEA sentence by comma and returns fields with trimming.
    /// </summary>
    public static string[] SplitNmea(this string sentence)
    {
        if (string.IsNullOrEmpty(sentence))
            return new string[0];

        return sentence.Split(',').Select(s => s.Trim()).ToArray();
    }

    /// <summary>
    /// Extracts NMEA checksum from sentence (after * character).
    /// </summary>
    public static string GetNmeaChecksum(this string sentence)
    {
        var parts = sentence.Split('*');
        return parts.Length == 2 ? parts[1] : string.Empty;
    }

    /// <summary>
    /// Removes NMEA checksum from sentence.
    /// </summary>
    public static string RemoveNmeaChecksum(this string sentence)
    {
        var parts = sentence.Split('*');
        return parts[0];
    }

    /// <summary>
    /// Validates IMEI format (15-20 digits).
    /// </summary>
    public static bool IsValidImei(this string imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
            return false;

        return imei.Length >= 15 && imei.Length <= 20 && imei.All(char.IsDigit);
    }

    /// <summary>
    /// Validates device ID format (non-empty, alphanumeric with dashes).
    /// </summary>
    public static bool IsValidDeviceId(this string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return false;

        return deviceId.Length <= 50 && deviceId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    /// <summary>
    /// Truncates string to maximum length with ellipsis.
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (value is null || value.Length <= maxLength)
            return value;

        return value.Substring(0, Math.Max(0, maxLength - suffix.Length)) + suffix;
    }

    /// <summary>
    /// Converts hex string to byte array.
    /// </summary>
    public static byte[] HexToByteArray(this string hexString)
    {
        if (string.IsNullOrWhiteSpace(hexString))
            return new byte[0];

        hexString = hexString.Replace("-", "").Replace(" ", "");

        if (hexString.Length % 2 != 0)
            return new byte[0];

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
            return new byte[0];
        }
    }

    /// <summary>
    /// Sanitizes device ID by removing/replacing invalid characters.
    /// </summary>
    public static string SanitizeDeviceId(this string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return "unknown";

        var sanitized = new string(deviceId
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized.Substring(0, Math.Min(50, sanitized.Length));
    }

    /// <summary>
    /// Checks if string is a valid hex color code.
    /// </summary>
    public static bool IsValidHexColor(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.TrimStart('#');
        return (value.Length == 6 || value.Length == 8) && value.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }
}
