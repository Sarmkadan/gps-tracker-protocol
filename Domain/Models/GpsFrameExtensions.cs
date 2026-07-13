#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Globalization;

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Provides useful extension methods for <see cref="GpsFrame"/> class.
/// </summary>
public static class GpsFrameExtensions
{
    /// <summary>
    /// Parses the timestamp from the GPS frame based on protocol type.
    /// </summary>
    /// <param name="frame">The GPS frame to parse timestamp from.</param>
    /// <returns>DateTime representing the parsed timestamp, or null if parsing fails or frame is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="frame"/> is null.</exception>
    public static DateTime? ParseTimestamp(this GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 10)
            return null;

        return frame.Protocol switch
        {
            ProtocolType.GT06 => ParseGt06Timestamp(frame),
            ProtocolType.H02 => ParseH02Timestamp(frame),
            ProtocolType.TK103 => ParseTk103Timestamp(frame),
            _ => null
        };
    }

    /// <summary>
    /// Gets the device identifier from the GPS frame based on protocol type.
    /// </summary>
    /// <param name="frame">The GPS frame to extract device ID from.</param>
    /// <returns>Device identifier string, or null if not found or frame is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="frame"/> is null.</exception>
    public static string? GetDeviceId(this GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        return frame.Protocol switch
        {
            ProtocolType.GT06 => frame.ExtractString(2, 10),
            ProtocolType.H02 => frame.ExtractString(4, 12),
            ProtocolType.TK103 => frame.ExtractString(2, 15),
            _ => null
        };
    }

    /// <summary>
    /// Calculates the signal strength percentage from the GPS frame.
    /// </summary>
    /// <param name="frame">The GPS frame to calculate signal strength from.</param>
    /// <returns>Signal strength percentage (0-100), or null if not available or frame is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="frame"/> is null.</exception>
    public static int? GetSignalStrength(this GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 20)
            return null;

        return frame.Protocol switch
        {
            ProtocolType.GT06 => ParseGt06SignalStrength(frame),
            ProtocolType.H02 => ParseH02SignalStrength(frame),
            ProtocolType.TK103 => ParseTk103SignalStrength(frame),
            _ => null
        };
    }

    /// <summary>
    /// Creates a human-readable summary of the GPS frame data.
    /// </summary>
    /// <param name="frame">The GPS frame to summarize.</param>
    /// <param name="includeRawData">Whether to include raw data in the summary.</param>
    /// <returns>Formatted summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="frame"/> is null.</exception>
    public static string ToDiagnosticString(this GpsFrame frame, bool includeRawData = false)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== GPS Frame Diagnostic Report ===");
        sb.AppendLine($"Frame ID: {frame.FrameId}");
        sb.AppendLine($"Protocol: {frame.Protocol}");
        sb.AppendLine($"Received At: {frame.ReceivedAt:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"Source: {frame.SourceAddress}:{frame.SourcePort}");
        sb.AppendLine($"Status: {(frame.IsValid() ? "VALID" : "INVALID")}");
        sb.AppendLine($"Checksum: {frame.ChecksumValue ?? "N/A"}");
        sb.AppendLine($"Raw Data Length: {frame.RawData.Length} bytes");

        if (frame.Headers.Count > 0)
        {
            sb.AppendLine("Headers:");
            foreach (var header in frame.Headers)
            {
                sb.AppendLine($" {header.Key}: {header.Value}");
            }
        }

        var timestamp = frame.ParseTimestamp();
        if (timestamp.HasValue)
        {
            sb.AppendLine($"Device ID: {frame.GetDeviceId()}");
            sb.AppendLine($"Timestamp: {timestamp.Value:yyyy-MM-dd HH:mm:ss}");
        }

        var signalStrength = frame.GetSignalStrength();
        if (signalStrength.HasValue)
        {
            sb.AppendLine($"Signal Strength: {signalStrength.Value}%");
        }

        if (includeRawData && frame.RawData.Length > 0)
        {
            sb.AppendLine("Raw Data (hex):");
            sb.AppendLine(frame.ToHex());
        }

        sb.AppendLine("=== End of Report ===");
        return sb.ToString();
    }

    private static DateTime? ParseGt06Timestamp(GpsFrame frame)
    {
        // GT06 timestamp format: YYMMDDHHmmSS
        // Starts at byte offset 7, 6 bytes total
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 13)
            return null;

        try
        {
            var year = 2000 + int.Parse(frame.ExtractString(7, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var month = int.Parse(frame.ExtractString(9, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var day = int.Parse(frame.ExtractString(11, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var hour = int.Parse(frame.ExtractString(13, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var minute = int.Parse(frame.ExtractString(15, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var second = int.Parse(frame.ExtractString(17, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentOutOfRangeException or OverflowException)
        {
            return null;
        }
    }

    private static DateTime? ParseH02Timestamp(GpsFrame frame)
    {
        // H02 timestamp format: YYYYMMDDHHmmSS
        // Starts at byte offset 16, 6 bytes total
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 22)
            return null;

        try
        {
            var year = int.Parse(frame.ExtractString(16, 4), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var month = int.Parse(frame.ExtractString(20, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var day = int.Parse(frame.ExtractString(22, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var hour = int.Parse(frame.ExtractString(24, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var minute = int.Parse(frame.ExtractString(26, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var second = int.Parse(frame.ExtractString(28, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentOutOfRangeException or OverflowException)
        {
            return null;
        }
    }

    private static DateTime? ParseTk103Timestamp(GpsFrame frame)
    {
        // TK103 timestamp format: YYYY-MM-DD HH:mm:SS
        // Starts at byte offset 5, 19 bytes total
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 24)
            return null;

        var datePart = frame.ExtractString(5, 10); // YYYY-MM-DD
        var timePart = frame.ExtractString(16, 8); // HH:mm:SS

        if (DateTime.TryParseExact($"{datePart} {timePart}", "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
        {
            return result;
        }

        return null;
    }

    private static int ParseGt06SignalStrength(GpsFrame frame)
    {
        // GT06 signal strength at byte offset 23 (0-31 scale, convert to 0-100%)
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 24)
            return 0;

        var signalValue = frame.RawData[23];
        return Math.Clamp((signalValue * 100) / 31, 0, 100);
    }

    private static int ParseH02SignalStrength(GpsFrame frame)
    {
        // H02 signal strength at byte offset 30 (0-99 scale)
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 31)
            return 0;

        return Math.Clamp((int)frame.RawData[30], 0, 100);
    }

    private static int ParseTk103SignalStrength(GpsFrame frame)
    {
        // TK103 signal strength at byte offset 25 (0-31 scale, convert to 0-100%)
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.RawData.Length < 26)
            return 0;

        var signalValue = frame.RawData[25];
        return Math.Clamp((signalValue * 100) / 31, 0, 100);
    }
}