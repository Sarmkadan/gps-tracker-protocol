#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// GT06 protocol parser implementation
// =====================================================================

namespace GpsTrackerProtocol.Parsers;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol;

/// <summary>
/// Parser for GT06 protocol frames.
/// </summary>
public class Gt06ProtocolParser : IProtocolParser
{
    /// <inheritdoc/>
    public ProtocolType ProtocolType => ProtocolType.GT06;

    /// <inheritdoc/>
    public string ParserName => "GT06 Protocol Parser";

    /// <inheritdoc/>
    public ParseResult<LocationData> Parse(ReadOnlySpan<byte> frameData)
    {
        try
        {
            // Validate minimum frame size before any access
            if (frameData.Length < ProtocolConstants.GT06_MIN_FRAME_SIZE)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_FRAME",
                    $"GT06 frame too short: expected at least {ProtocolConstants.GT06_MIN_FRAME_SIZE} bytes, got {frameData.Length} bytes",
                    0,
                    ProtocolType.GT06
                );
            }

            // Validate maximum frame size to prevent allocation attacks
            if (frameData.Length > ProtocolConstants.GT06_MAX_FRAME_SIZE)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_FRAME",
                    $"GT06 frame too large: maximum {ProtocolConstants.GT06_MAX_FRAME_SIZE} bytes, got {frameData.Length} bytes",
                    0,
                    ProtocolType.GT06
                );
            }

            // Validate start markers
            if (frameData[0] != ProtocolConstants.GT06_START_MARKER && frameData[0] != ProtocolConstants.GT06_EXTENDED_START_MARKER)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_START_MARKER",
                    $"GT06 invalid start marker: expected 0x78 or 0x79, got 0x{frameData[0]:X2}",
                    0,
                    ProtocolType.GT06
                );
            }

            // Validate stop markers (last two bytes must be 0x0D 0x0A)
            if (frameData[^2] != ProtocolConstants.GT06_END_MARKER || frameData[^1] != 0x0A)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_STOP_MARKERS",
                    "GT06 invalid stop markers: expected 0x0D 0x0A at end of frame",
                    frameData.Length - 2,
                    ProtocolType.GT06
                );
            }

            // Validate checksum
            if (!ValidateChecksum(frameData))
            {
                return ParseResult<LocationData>.Failure(
                    "CHECKSUM_FAILED",
                    "GT06 checksum validation failed",
                    frameData.Length - 3,
                    ProtocolType.GT06
                );
            }

            // Extract declared length from frame (byte at index 2)
            byte declaredLength = frameData[2];

            // Validate that declared length is reasonable (prevent allocation attacks)
            if (declaredLength > ProtocolConstants.GT06_MAX_FRAME_SIZE - 7)
            {
                return ParseResult<LocationData>.Failure(
                    "INVALID_LENGTH",
                    $"GT06 declared length too large: {declaredLength} bytes",
                    2,
                    ProtocolType.GT06
                );
            }

            // Validate declared length against actual buffer size
            int minimumRequiredLength = declaredLength + 7;
            if (frameData.Length < minimumRequiredLength)
            {
                return ParseResult<LocationData>.Failure(
                    "LENGTH_MISMATCH",
                    $"GT06 declared length mismatch: declared {declaredLength} bytes of data, but frame has {frameData.Length} bytes total. Minimum required: {minimumRequiredLength} bytes",
                    2,
                    ProtocolType.GT06
                );
            }

            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frameData),
                Protocol = ProtocolType.GT06,
                Timestamp = DateTime.UtcNow // Will be updated with extracted timestamp
            };

            // Extract GPS data from specific byte positions with bounds checking
            if (frameData.Length >= 30)
            {
                location.Timestamp = ExtractTimestamp(frameData, 5);
                // data[20] is assumed to be the "Course and Status" byte based on GT06 protocol variations,
                // specifically containing hemisphere information (Bit 2 for Latitude, Bit 3 for Longitude).
                // This byte is located between the 1-byte Speed field (data[19]) and the 2-byte Bearing field (data[21-22]).
                byte statusByte = frameData[20];
                location.Latitude = ExtractCoordinate(frameData, 11, true, statusByte);
                location.Longitude = ExtractCoordinate(frameData, 15, false, statusByte);
                location.Speed = ExtractSpeed(frameData, 19);
                location.Bearing = ExtractBearing(frameData, 21);
                location.SatelliteCount = frameData[23];
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
                    ProtocolType.GT06
                );
            }

            return ParseResult<LocationData>.Success(location);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException
            or IndexOutOfRangeException or InvalidOperationException or GpsTrackerException)
        {
            return ParseResult<LocationData>.Failure(
                "PARSE_ERROR",
                $"GT06 parsing failed: {ex.Message}",
                0,
                ProtocolType.GT06
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
            if (frameData.Length < ProtocolConstants.GT06_MIN_FRAME_SIZE)
                return false;

            if (frameData.Length > ProtocolConstants.GT06_MAX_FRAME_SIZE)
                return false;

            if (frameData[0] != ProtocolConstants.GT06_START_MARKER && frameData[0] != ProtocolConstants.GT06_EXTENDED_START_MARKER)
                return false;

            if (frameData[^2] != ProtocolConstants.GT06_END_MARKER || frameData[^1] != 0x0A)
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
        // A valid GT06 frame must have at least 7 bytes:
        // 2 Start + 1 Packet Length + 1 Protocol Number + 1 Information Content (min) + 1 Checksum + 2 Stop.
        if (frameData.Length < 7)
        {
            return false;
        }

        // The checksum byte is located at frameData[frameData.Length - 3].
        // The stop bytes are at frameData[frameData.Length - 2] and frameData[frameData.Length - 1].
        byte expectedChecksum = frameData[frameData.Length - 3];

        // Calculate XOR sum from frameData[2] (Packet Length) up to frameData[frameData.Length - 4]
        byte calculatedChecksum = 0;
        for (int i = 2; i <= frameData.Length - 4; i++)
        {
            calculatedChecksum ^= frameData[i];
        }

        return calculatedChecksum == expectedChecksum;
    }

    private string ExtractDeviceId(ReadOnlySpan<byte> frameData)
    {
        if (frameData.Length >= 9)
        {
            // Device ID is 5 ASCII characters starting at index 4
            Span<char> deviceIdChars = stackalloc char[5];
            for (int i = 0; i < 5; i++)
            {
                deviceIdChars[i] = (char)frameData[4 + i];
            }
            return new string(deviceIdChars).TrimEnd('\0');
        }
        return "unknown";
    }

    private DateTime ExtractTimestamp(ReadOnlySpan<byte> frameData, int offset)
    {
        // GT06 timestamp format: year (offset), month (offset+1), day (offset+2),
        // hour (offset+3), minute (offset+4), second (offset+5)
        var year = 2000 + frameData[offset];
        var month = frameData[offset + 1];
        var day = frameData[offset + 2];
        var hour = frameData[offset + 3];
        var minute = frameData[offset + 4];
        var second = frameData[offset + 5];
        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }

    private double ExtractCoordinate(ReadOnlySpan<byte> frameData, int offset, bool isLatitude, byte statusByte)
    {
        // GT06 coordinates are stored as 4-byte unsigned integers representing 1/1000000 degrees
        uint raw = (uint)((frameData[offset] << 24) | (frameData[offset + 1] << 16) | (frameData[offset + 2] << 8) | frameData[offset + 3]);
        double degrees = raw / 1_000_000.0;

        if (isLatitude)
        {
            // Bit 2: Latitude Hemisphere (0: South, 1: North)
            bool isNorth = (statusByte & 0b00000100) != 0;
            if (!isNorth)
                degrees = -degrees;
        }
        else
        {
            // Bit 3: Longitude Hemisphere (0: East, 1: West)
            bool isWest = (statusByte & 0b00001000) != 0;
            if (isWest)
                degrees = -degrees;
        }

        return degrees;
    }

    private double ExtractSpeed(ReadOnlySpan<byte> frameData, int offset)
    {
        // Speed is stored as 2-byte unsigned integer representing 0.1 knots
        ushort speedRaw = (ushort)((frameData[offset] << 8) | frameData[offset + 1]);
        return speedRaw * 0.1 * 1.852; // Convert to km/h
    }

    private double ExtractBearing(ReadOnlySpan<byte> frameData, int offset)
    {
        // Bearing is stored as 2-byte unsigned integer representing 0.1 degrees
        ushort bearingRaw = (ushort)((frameData[offset] << 8) | frameData[offset + 1]);
        return bearingRaw / 10.0;
    }
}
