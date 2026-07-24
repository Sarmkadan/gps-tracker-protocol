#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// TK103 protocol parser implementation
// =====================================================================

namespace GpsTrackerProtocol.Parsers;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol;

/// <summary>
/// Parser for TK103 protocol frames.
/// </summary>
public class Tk103ProtocolParser : IProtocolParser
{
    /// <inheritdoc/>
    public ProtocolType ProtocolType => ProtocolType.TK103;

    /// <inheritdoc/>
    public string ParserName => "TK103 Protocol Parser";

    /// <inheritdoc/>
    public ParseResult<LocationData> Parse(ReadOnlySpan<byte> frameData)
    {
        try
        {
            // Validate minimum frame size before any parsing
            if (frameData.Length < ProtocolConstants.TK103_MIN_FRAME_SIZE)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_FRAME",
                    $"TK103 frame too short: expected at least {ProtocolConstants.TK103_MIN_FRAME_SIZE} bytes, got {frameData.Length} bytes",
                    0,
                    ProtocolType.TK103
                );
            }

            // Validate maximum frame size to prevent allocation attacks
            if (frameData.Length > ProtocolConstants.TK103_MAX_FRAME_SIZE)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_FRAME",
                    $"TK103 frame too large: maximum {ProtocolConstants.TK103_MAX_FRAME_SIZE} bytes, got {frameData.Length} bytes",
                    0,
                    ProtocolType.TK103
                );
            }

            // Validate start marker
            if (frameData[0] != ProtocolConstants.TK103_START_MARKER)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_START_MARKER",
                    $"TK103 invalid start marker: expected 0x{ProtocolConstants.TK103_START_MARKER:X2}, got 0x{frameData[0]:X2}",
                    0,
                    ProtocolType.TK103
                );
            }

            // Convert to string for parsing (TK103 is ASCII-based)
            string frameStr = System.Text.Encoding.ASCII.GetString(frameData).Trim();

            // Validate checksum BEFORE parsing any data
            if (!ValidateChecksum(frameStr))
            {
                return ParseResult<LocationData>.Failure(
                    "CHECKSUM_FAILED",
                    "TK103 checksum validation failed",
                    0,
                    ProtocolType.TK103
                );
            }

            // Basic validation of frame structure
            if (frameStr.Length < 30)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_STRUCTURE",
                    "TK103 frame structure too short",
                    0,
                    ProtocolType.TK103
                );
            }

            var parts = frameStr.Split(',');

            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frameStr),
                Protocol = ProtocolType.TK103,
                Timestamp = DateTime.UtcNow // Will be updated with extracted timestamp
            };

            if (parts.Length >= 8)
            {
                // Format: (IMEI),YYYYMMDDHHMMSS,lat,NS,lon,EW,speed,course
                try
                {
                    location.Timestamp = DateTime.ParseExact(parts[1], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal);
                }
                catch
                {
                    // If parsing fails, keep current time
                }

                location.Latitude = ParseCoordinate(parts[2], parts[3]);
                location.Longitude = ParseCoordinate(parts[4], parts[5]);
                location.Speed = double.Parse(parts[6]);
                location.Bearing = double.Parse(parts[7]);
            }
            else
            {
                // Use current time if we can't extract timestamp from frame
                location.Timestamp = DateTime.UtcNow;
            }

            if (!location.IsValid())
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_DATA",
                    "Location data validation failed: invalid coordinates or required fields",
                    0,
                    ProtocolType.TK103
                );
            }

            return ParseResult<LocationData>.Success(location);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException
            or IndexOutOfRangeException or InvalidOperationException or GpsTrackerException)
        {
            return ParseResult<LocationData>.Failure(
                "PARSE_ERROR",
                $"TK103 parsing failed: {ex.Message}",
                0,
                ProtocolType.TK103
            );
        }
    }

    /// <inheritdoc/>
    public ParseResult<LocationData> Parse(GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        return Parse(frame.RawData.AsSpan());
    }

    /// <inheritdoc/>
    public bool Validate(ReadOnlySpan<byte> frameData)
    {
        try
        {
            if (frameData.Length < ProtocolConstants.TK103_MIN_FRAME_SIZE)
                return false;

            if (frameData.Length > ProtocolConstants.TK103_MAX_FRAME_SIZE)
                return false;

            if (frameData[0] != ProtocolConstants.TK103_START_MARKER)
                return false;

            // Convert to string for checksum validation and structure checking
            string frameStr = System.Text.Encoding.ASCII.GetString(frameData).Trim();

            // Validate checksum
            if (!ValidateChecksum(frameStr))
                return false;

            // Check for expected format: (IMEI),timestamp,lat,NS,lon,EW,speed,course
            var parts = frameStr.Split(',');
            if (parts.Length < 8)
                return false;

            // Validate IMEI format (15 digits)
            string imei = parts[0].Trim('(', ')');
            if (imei.Length != 15 || !imei.All(char.IsDigit))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates TK103 frame checksum.
    /// </summary>
    /// <param name="frameStr">The TK103 frame as a string.</param>
    /// <returns>True if checksum is valid, false otherwise.</returns>
    private bool ValidateChecksum(string frameStr)
    {
        // TK103 frames use NMEA-style checksum with '*' delimiter
        // Format: (IMEI),data,*checksum

        int checksumDelimiterIndex = frameStr.IndexOf('*');

        if (checksumDelimiterIndex == -1)
        {
            // No checksum present - invalid frame
            return false;
        }

        // Extract the data part for checksum calculation (before '*')
        string dataForChecksum = frameStr.Substring(0, checksumDelimiterIndex);

        // Calculate checksum: XOR of all bytes in the data part
        byte calculatedChecksum = 0;
        foreach (char c in dataForChecksum)
        {
            calculatedChecksum ^= (byte)c;
        }

        // Extract the provided checksum (two hex digits after '*')
        if (checksumDelimiterIndex + 3 > frameStr.Length)
        {
            // Checksum part is too short
            return false;
        }

        string providedChecksumHex = frameStr.Substring(checksumDelimiterIndex + 1, 2);

        if (!byte.TryParse(providedChecksumHex, System.Globalization.NumberStyles.HexNumber, null, out byte providedChecksum))
        {
            // Invalid hexadecimal checksum string
            return false;
        }

        return calculatedChecksum == providedChecksum;
    }

    private string ExtractDeviceId(string frameStr)
    {
        // Format: (IMEI),timestamp,lat,NS,lon,EW,speed,course
        var parts = frameStr.Split(',');
        return parts.Length > 0 ? parts[0].Trim('(', ')') : "unknown";
    }

    private double ParseCoordinate(string value, string direction)
    {
        if (!double.TryParse(value, out var coord))
            return 0;

        var degrees = Math.Floor(coord / 100);
        var minutes = coord - (degrees * 100);
        var coordinate = degrees + (minutes / 60);

        if (direction == "S" || direction == "W")
            coordinate = -coordinate;

        return coordinate;
    }
}
