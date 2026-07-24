#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// H02 protocol parser implementation
// =====================================================================

namespace GpsTrackerProtocol.Parsers;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol;

/// <summary>
/// Parser for H02 protocol frames (NMEA and proprietary formats).
/// </summary>
public class H02ProtocolParser : IProtocolParser
{
    /// <inheritdoc/>
    public ProtocolType ProtocolType => ProtocolType.H02;

    /// <inheritdoc/>
    public string ParserName => "H02 Protocol Parser";

    /// <inheritdoc/>
    public ParseResult<LocationData> Parse(ReadOnlySpan<byte> frameData)
    {
        try
        {
            // Validate minimum frame size before any parsing
            if (frameData.Length < ProtocolConstants.H02_MIN_FRAME_SIZE)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_FRAME",
                    $"H02 frame too short: expected at least {ProtocolConstants.H02_MIN_FRAME_SIZE} bytes, got {frameData.Length} bytes",
                    0,
                    ProtocolType.H02
                );
            }

            // Validate maximum frame size to prevent allocation attacks
            if (frameData.Length > ProtocolConstants.H02_MAX_FRAME_SIZE)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_FRAME",
                    $"H02 frame too large: maximum {ProtocolConstants.H02_MAX_FRAME_SIZE} bytes, got {frameData.Length} bytes",
                    0,
                    ProtocolType.H02
                );
            }

            // Validate checksum BEFORE parsing any data
            if (!ValidateChecksum(frameData))
            {
                return ParseResult<LocationData>.Failure(
                    "CHECKSUM_FAILED",
                    "H02 checksum validation failed",
                    frameData.Length - 3,
                    ProtocolType.H02
                );
            }

            // Convert to string for parsing
            string frameStr = System.Text.Encoding.ASCII.GetString(frameData);
            var parts = frameStr.Split(',');

            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frameStr),
                Protocol = ProtocolType.H02,
                Timestamp = DateTime.UtcNow // Will be updated with extracted timestamp
            };

            if (frameStr.StartsWith(ProtocolConstants.H02_HQ_START_MARKER))
            {
                // *HQ,{IMEI},V1,{HHMMSS},{lat},{NS},{lon},{EW},{speed},{bearing},{DDMMYY},...
                if (parts.Length >= 10)
                {
                    // Parse timestamp: DDMMYY + HHMMSS
                    if (parts.Length > 10 && parts[10].Length >= 6 && parts[3].Length >= 6)
                    {
                        string datePart = parts[10][..6];
                        string timePart = parts[3][..6];
                        try
                        {
                            location.Timestamp = DateTime.ParseExact(datePart + timePart, "ddMMyyHHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal);
                        }
                        catch
                        {
                            // If parsing fails, keep current time
                        }
                    }

                    location.Latitude = ParseCoordinate(parts[4], parts[5]);
                    // Use the E/W indicator at parts[7], not parts[6] which is the longitude value.
                    location.Longitude = ParseCoordinate(parts[6], parts[7]);
                    location.Speed = double.Parse(parts[8]);
                    location.Bearing = double.Parse(parts[9]);
                }
            }
            else if (parts.Length >= 9)
            {
                // $GPRMC,{HHMMSS.ss},{A/V},{lat},{NS},{lon},{EW},{speed},{bearing},{DDMMYY},...
                var timeStr = parts[1].Length >= 6 ? parts[1][..6] : parts[1];
                if (parts.Length > 9 && parts[9].Length >= 6)
                {
                    string datePart = parts[9][..6];
                    try
                    {
                        location.Timestamp = DateTime.ParseExact(datePart + timeStr, "ddMMyyHHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal);
                    }
                    catch
                    {
                        // If parsing fails, keep current time
                    }
                }

                location.Latitude = ParseCoordinate(parts[3], parts[4]);
                location.Longitude = ParseCoordinate(parts[5], parts[6]);
                location.Speed = double.Parse(parts[7]);
                location.Bearing = double.Parse(parts[8]);
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
                    ProtocolType.H02
                );
            }

            return ParseResult<LocationData>.Success(location);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException
            or IndexOutOfRangeException or InvalidOperationException or GpsTrackerException)
        {
            return ParseResult<LocationData>.Failure(
                "PARSE_ERROR",
                $"H02 parsing failed: {ex.Message}",
                0,
                ProtocolType.H02
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
            if (frameData.Length < ProtocolConstants.H02_MIN_FRAME_SIZE)
                return false;

            if (frameData.Length > ProtocolConstants.H02_MAX_FRAME_SIZE)
                return false;

            // Check if it looks like H02 format (starts with $ or *HQ)
            string frameStr = System.Text.Encoding.ASCII.GetString(frameData);
            if (!frameStr.StartsWith("$") && !frameStr.StartsWith("*HQ"))
                return false;

            return ValidateChecksum(frameData);
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateChecksum(ReadOnlySpan<byte> frameData)
    {
        string frameStr = System.Text.Encoding.ASCII.GetString(frameData).Trim();

        // H02 protocol is typically NMEA-like with a checksum after '*'
        int startDelimiterIndex = frameStr.IndexOf('$');
        int checksumDelimiterIndex = frameStr.IndexOf('*');

        if (startDelimiterIndex == -1 || checksumDelimiterIndex == -1 || checksumDelimiterIndex < startDelimiterIndex)
        {
            // Invalid H02 frame format or no checksum present
            return false;
        }

        // Extract the data part for checksum calculation (between '$' and '*')
        string dataForChecksum = frameStr.Substring(startDelimiterIndex + 1, checksumDelimiterIndex - startDelimiterIndex - 1);
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
        if (frameStr.StartsWith(ProtocolConstants.H02_HQ_START_MARKER))
        {
            // *HQ,{IMEI},V1,...
            var parts = frameStr.Split(',');
            return parts.Length > 1 ? parts[1] : "unknown";
        }
        else
        {
            // $GPRMC,... - device ID is typically the first field after header
            var parts = frameStr.Split(',');
            return parts.Length > 0 ? parts[0] : "unknown";
        }
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
