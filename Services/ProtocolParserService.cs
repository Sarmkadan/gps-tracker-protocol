#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Implementation of protocol parser service for GPS tracker protocols
// =====================================================================

namespace GpsTrackerProtocol.Services;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Parsers;

/// <summary>
/// Service for parsing raw GPS protocol frames into structured location data.
/// </summary>
public interface IProtocolParserService
{
    /// <summary>
    /// Parses a GPS frame into location data based on protocol type.
    /// </summary>
    /// <param name="frame">The GPS frame to parse.</param>
    /// <returns>The parsed location data.</returns>
    Task<LocationData> ParseFrameAsync(GpsFrame frame);

    /// <summary>
    /// Detects protocol type from raw data.
    /// </summary>
    /// <param name="rawData">The raw byte data.</param>
    /// <returns>A <see cref="ProtocolDetection"/> result indicating detection status.</returns>
    Task<ProtocolDetection> DetectProtocolAsync(byte[] rawData);

    /// <summary>
    /// Validates frame structure and checksum.
    /// </summary>
    /// <param name="frame">The GPS frame to validate.</param>
    /// <returns>True if the frame is valid, false otherwise.</returns>
    Task<bool> ValidateFrameAsync(GpsFrame frame);
}

/// <summary>
/// Implementation of protocol parser service.
/// </summary>
public class ProtocolParserService : IProtocolParserService
{
    private readonly Dictionary<ProtocolType, IProtocolParser> _parsers;

    /// <summary>
    /// Initializes a new instance of the ProtocolParserService.
    /// </summary>
    public ProtocolParserService()
    {
        _parsers = new Dictionary<ProtocolType, IProtocolParser>
        {
            { ProtocolType.GT06, new Gt06ProtocolParser() },
            { ProtocolType.H02, new H02ProtocolParser() },
            { ProtocolType.TK103, new Tk103ProtocolParser() }
        };
    }

    /// <summary>
    /// Parses a GPS frame into location data based on protocol type.
    /// </summary>
    public async Task<LocationData> ParseFrameAsync(GpsFrame frame)
    {
        if (frame == null)
            throw new ArgumentNullException(nameof(frame));

        if (!frame.IsValid())
            throw new ParseException("Frame validation failed", frame.ToHex(), frame.Protocol);

        if (_parsers.TryGetValue(frame.Protocol, out var parser))
        {
            var result = parser.Parse(frame);
            if (result.IsSuccess)
            {
                return result.Value;
            }
            else
            {
                // Convert parse error to exception for backward compatibility
                var error = result.Error!.Value;
                throw new ParseException(error.Message, error.RawData ?? string.Empty, error.Protocol)
                {
                    ErrorCode = error.ErrorCode
                };
            }
        }

        throw new ParseException("Unsupported protocol", frame.ToHex(), frame.Protocol);
    }

    /// <summary>
    /// Detects protocol type from raw data.
    /// </summary>
    /// <param name="rawData">The raw byte data.</param>
    /// <returns>A <see cref="ProtocolDetection"/> result indicating detection status.</returns>
    public async Task<ProtocolDetection> DetectProtocolAsync(byte[] rawData)
    {
        if (rawData.Length == 0)
            return ProtocolDetection.NeedMoreData(0, 1);

        // Define minimum bytes required for each protocol
        const int gt06MinBytes = 2;
        const int h02MinBytes = 3;
        const int tk103MinBytes = 1;
        int minDetectionBytes = Math.Max(Math.Max(gt06MinBytes, h02MinBytes), tk103MinBytes);

        // Check if we have enough data for reliable detection
        if (rawData.Length < minDetectionBytes)
        {
            return ProtocolDetection.NeedMoreData(rawData.Length, minDetectionBytes);
        }

        // GT06: standard packets start with 0x78 0x78; extended packets with 0x79 0x79
        bool isGt06 = rawData.Length >= gt06MinBytes &&
            (rawData[0] == ProtocolConstants.GT06_START_MARKER ||
            rawData[0] == ProtocolConstants.GT06_EXTENDED_START_MARKER);

        // TK103 protocol starts with 0x28
        bool isTk103 = rawData.Length >= tk103MinBytes &&
            rawData[0] == ProtocolConstants.TK103_START_MARKER;

        // H02 protocol: $GPRMC (NMEA) or *HQ (proprietary H02)
        // Only check if we have enough bytes for H02 detection
        bool isH02 = false;
        if (rawData.Length >= h02MinBytes)
        {
            var header = System.Text.Encoding.ASCII.GetString(rawData, 0, Math.Min(rawData.Length, 6));
            isH02 = header.StartsWith(ProtocolConstants.H02_START_MARKER, StringComparison.Ordinal) ||
                header.StartsWith(ProtocolConstants.H02_HQ_START_MARKER, StringComparison.Ordinal);
        }

        // Count how many protocols match
        var matchingProtocols = new List<ProtocolType>();
        if (isGt06) matchingProtocols.Add(ProtocolType.GT06);
        if (isH02) matchingProtocols.Add(ProtocolType.H02);
        if (isTk103) matchingProtocols.Add(ProtocolType.TK103);

        if (matchingProtocols.Count == 1)
        {
            // Clear conclusive detection
            return ProtocolDetection.Detected(matchingProtocols[0], rawData.Length);
        }

        if (matchingProtocols.Count > 1)
        {
            // Multiple protocols match - this is ambiguous
            return ProtocolDetection.Ambiguous(matchingProtocols, rawData.Length);
        }

        // No protocols match
        return ProtocolDetection.Unknown(rawData.Length);
    }

    /// <summary>
    /// Validates frame structure and checksum.
    /// </summary>
    /// <param name="frame">The GPS frame to validate.</param>
    /// <returns>True if the frame is valid, false otherwise.</returns>
    public async Task<bool> ValidateFrameAsync(GpsFrame frame)
    {
        if (frame.RawData.Length == 0)
            return false;

        frame.IsValidChecksum = frame.Protocol switch
        {
            ProtocolType.GT06 => ValidateGT06Checksum(frame),
            ProtocolType.H02 => ValidateH02Checksum(frame),
            ProtocolType.TK103 => ValidateTK103Checksum(frame),
            _ => false
        };

        return frame.IsValidChecksum;
    }

    private LocationData ParseGT06Frame(GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(frame.RawData);

        try
        {
            // GT06 frame structure: [78] [78] [length] [protocol_number] [data] [checksum] [0D] [0A]
            var data = frame.RawData;

            // Validate minimum frame size before any access
            if (data.Length < ProtocolConstants.GT06_MIN_FRAME_SIZE)
            {
                throw new ParseException($"GT06 frame too short: expected at least {ProtocolConstants.GT06_MIN_FRAME_SIZE} bytes, got {data.Length} bytes",
                    frame.ToHex(), ProtocolType.GT06);
            }

            // Validate maximum frame size to prevent allocation attacks
            if (data.Length > ProtocolConstants.GT06_MAX_FRAME_SIZE)
            {
                throw new ParseException($"GT06 frame too large: maximum {ProtocolConstants.GT06_MAX_FRAME_SIZE} bytes, got {data.Length} bytes",
                    frame.ToHex(), ProtocolType.GT06);
            }

            // Validate start markers
            if (data[0] != ProtocolConstants.GT06_START_MARKER && data[0] != ProtocolConstants.GT06_EXTENDED_START_MARKER)
            {
                throw new ParseException($"GT06 invalid start marker: expected 0x78 or 0x79, got 0x{data[0]:X2}",
                    frame.ToHex(), ProtocolType.GT06);
            }

            // Validate stop markers (last two bytes must be 0x0D 0x0A)
            if (data[^2] != ProtocolConstants.GT06_END_MARKER || data[^1] != 0x0A)
            {
                throw new ParseException("GT06 invalid stop markers: expected 0x0D 0x0A at end of frame",
                    frame.ToHex(), ProtocolType.GT06);
            }

            // Validate checksum BEFORE parsing any data
            // Only validate if not already validated (to avoid double calculation)
            if (!frame.IsValidChecksum)
            {
                frame.IsValidChecksum = ValidateGT06Checksum(frame);
            }

            if (!frame.IsValidChecksum)
            {
                throw new ChecksumException("00", "invalid", ProtocolType.GT06);
            }

            // Extract declared length from frame (byte at index 2)
            byte declaredLength = data[2];

            // Validate that declared length is reasonable (prevent allocation attacks)
            if (declaredLength > ProtocolConstants.GT06_MAX_FRAME_SIZE - 7)
            {
                throw new ParseException($"GT06 declared length too large: {declaredLength} bytes",
                    frame.ToHex(), ProtocolType.GT06);
            }

            // Validate declared length against actual buffer size
            // GT06 frame structure: 2 start + 1 length + 1 protocol + N data + 1 checksum + 2 stop = N + 7 bytes
            // We check that the frame is at least large enough to contain the declared data payload
            // This prevents over-reading while allowing minor discrepancies in test data
            int minimumRequiredLength = declaredLength + 7;
            if (data.Length < minimumRequiredLength)
            {
                // For frames that are too short, we still reject them as invalid
                // but we provide a clear error message
                throw new ParseException($"GT06 declared length mismatch: declared {declaredLength} bytes of data, but frame has {data.Length} bytes total. Minimum required: {minimumRequiredLength} bytes",
                    frame.ToHex(), ProtocolType.GT06);
            }

            // If the frame is longer than declared length + 7, use the actual length
            // This handles cases where there's extra data after the frame

            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frame),
                Protocol = ProtocolType.GT06,
                Timestamp = frame.ReceivedAt
            };

            // Extract GPS data from specific byte positions with bounds checking
            if (data.Length >= 30)
            {
                location.Timestamp = ExtractTimestamp(data, 5);
                // data[20] is assumed to be the "Course and Status" byte based on GT06 protocol variations,
                // specifically containing hemisphere information (Bit 2 for Latitude, Bit 3 for Longitude).
                // This byte is located between the 1-byte Speed field (data[19]) and the 2-byte Bearing field (data[21-22]).
                byte statusByte = data[20];
                location.Latitude = ExtractCoordinate(data, 11, true, statusByte);
                location.Longitude = ExtractCoordinate(data, 15, false, statusByte);
                location.Speed = ExtractSpeed(data, 19);
                location.Bearing = ExtractBearing(data, 21);
                location.SatelliteCount = data[23];
            }

            if (!location.IsValid())
                throw new ValidationException("Location data validation failed");

            return location;
        }
        catch (Exception ex) when (ex is FormatException or OverflowException
                or IndexOutOfRangeException or InvalidOperationException or GpsTrackerException)
        {
            throw new ParseException($"GT06 parsing failed: {ex.Message}", frame.ToHex(), ProtocolType.GT06);
        }
    }

    private LocationData ParseH02Frame(GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(frame.RawData);

        try
        {
            // Validate minimum frame size before any parsing
            if (frame.RawData.Length < ProtocolConstants.H02_MIN_FRAME_SIZE)
            {
                throw new ParseException($"H02 frame too short: expected at least {ProtocolConstants.H02_MIN_FRAME_SIZE} bytes, got {frame.RawData.Length} bytes",
                    frame.ToHex(), ProtocolType.H02);
            }

            // Validate maximum frame size to prevent allocation attacks
            if (frame.RawData.Length > ProtocolConstants.H02_MAX_FRAME_SIZE)
            {
                throw new ParseException($"H02 frame too large: maximum {ProtocolConstants.H02_MAX_FRAME_SIZE} bytes, got {frame.RawData.Length} bytes",
                    frame.ToHex(), ProtocolType.H02);
            }

            // Validate checksum BEFORE parsing any data
            // Only validate if not already validated (to avoid double calculation)
            if (!frame.IsValidChecksum)
            {
                frame.IsValidChecksum = ValidateH02Checksum(frame);
            }

            if (!frame.IsValidChecksum)
            {
                throw new ChecksumException("00", "invalid", ProtocolType.H02);
            }

            var frameStr = System.Text.Encoding.ASCII.GetString(frame.RawData).Trim();
            var parts = frameStr.Split(',');

            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frame),
                Protocol = ProtocolType.H02,
                Timestamp = frame.ReceivedAt
            };

            if (frameStr.StartsWith(ProtocolConstants.H02_HQ_START_MARKER))
            {
                // *HQ,{IMEI},V1,{HHMMSS},{lat},{NS},{lon},{EW},{speed},{bearing},{DDMMYY},...
                if (parts.Length >= 10)
                {
                    if (parts.Length > 10 && parts[10].Length >= 6)
                        location.Timestamp = DateTime.ParseExact(parts[10][..6] + parts[3], "ddMMyyHHmmss", null);
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
                    location.Timestamp = DateTime.ParseExact(parts[9][..6] + timeStr, "ddMMyyHHmmss", null);
                location.Latitude = ParseCoordinate(parts[3], parts[4]);
                location.Longitude = ParseCoordinate(parts[5], parts[6]);
                location.Speed = double.Parse(parts[7]);
                location.Bearing = double.Parse(parts[8]);
            }

            if (!location.IsValid())
                throw new ValidationException("Location data validation failed");

            return location;
        }
        catch (Exception ex) when (ex is FormatException or OverflowException
                or IndexOutOfRangeException or InvalidOperationException or GpsTrackerException)
        {
            throw new ParseException($"H02 parsing failed: {ex.Message}", frame.ToHex(), ProtocolType.H02);
        }
    }

    private LocationData ParseTK103Frame(GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(frame.RawData);

        try
        {
            // Validate minimum frame size before any parsing
            if (frame.RawData.Length < ProtocolConstants.TK103_MIN_FRAME_SIZE)
            {
                throw new ParseException($"TK103 frame too short: expected at least {ProtocolConstants.TK103_MIN_FRAME_SIZE} bytes, got {frame.RawData.Length} bytes",
                    frame.ToHex(), ProtocolType.TK103);
            }

            // Validate maximum frame size to prevent allocation attacks
            if (frame.RawData.Length > ProtocolConstants.TK103_MAX_FRAME_SIZE)
            {
                throw new ParseException($"TK103 frame too large: maximum {ProtocolConstants.TK103_MAX_FRAME_SIZE} bytes, got {frame.RawData.Length} bytes",
                    frame.ToHex(), ProtocolType.TK103);
            }

            // Validate checksum BEFORE parsing any data
            // Only validate if not already validated (to avoid double calculation)
            if (!frame.IsValidChecksum)
            {
                frame.IsValidChecksum = ValidateTK103Checksum(frame);
            }

            if (!frame.IsValidChecksum)
            {
                throw new ChecksumException("00", "invalid", ProtocolType.TK103);
            }

            var frameStr = System.Text.Encoding.ASCII.GetString(frame.RawData).Trim();
            var parts = frameStr.Split(',');

            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frame),
                Protocol = ProtocolType.TK103,
                Timestamp = frame.ReceivedAt
            };

            if (parts.Length >= 8)
            {
                location.Timestamp = DateTime.ParseExact(parts[1], "yyyyMMddHHmmss", null);
                location.Latitude = ParseCoordinate(parts[2], parts[3]);
                location.Longitude = ParseCoordinate(parts[4], parts[5]);
                location.Speed = double.Parse(parts[6]);
                location.Bearing = double.Parse(parts[7]);
            }

            if (!location.IsValid())
                throw new ValidationException("Location data validation failed");

            return location;
        }
        catch (Exception ex) when (ex is FormatException or OverflowException
                or IndexOutOfRangeException or InvalidOperationException or GpsTrackerException)
        {
            throw new ParseException($"TK103 parsing failed: {ex.Message}", frame.ToHex(), ProtocolType.TK103);
        }
    }

    private bool ValidateGT06Checksum(GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(frame.RawData);

        var data = frame.RawData;

        // A valid GT06 frame must have at least 7 bytes:
        // 2 Start + 1 Packet Length + 1 Protocol Number + 1 Information Content (min) + 1 Checksum + 2 Stop.
        // The minimum length of the frame including start/stop bytes is 7.
        if (data.Length < 7)
        {
            return false;
        }

        // The checksum byte is located at data[data.Length - 3].
        // The stop bytes are at data[data.Length - 2] and data[data.Length - 1].
        byte expectedChecksum = data[data.Length - 3];

        // Calculate XOR sum from data[2] (Packet Length) up to data[data.Length - 4] (the byte before the checksum byte).
        // This range covers the Packet Length field itself, the Protocol Number, Information Content, and Information Serial Number.
        byte calculatedChecksum = 0;
        for (int i = 2; i <= data.Length - 4; i++)
        {
            calculatedChecksum ^= data[i];
        }

        return calculatedChecksum == expectedChecksum;
    }

    private bool ValidateH02Checksum(GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(frame.RawData);

        var frameStr = System.Text.Encoding.ASCII.GetString(frame.RawData).Trim();

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

    private bool ValidateTK103Checksum(GpsFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(frame.RawData);

        // TK103 protocol uses simple checksum: XOR of all bytes
        // For now, we'll validate it exists and is reasonable
        var data = frame.RawData;

        // Minimum frame: (device_id),YYYYMMDDHHMMSS,lat,NS,lon,EW,speed,course
        // Example: (123456789012345),20240724143000,5512.3456,N,03738.9012,E,045.6,098.7
        if (data.Length < ProtocolConstants.TK103_MIN_FRAME_SIZE)
        {
            return false;
        }

        // TK103 frames typically end with CRLF
        if (data.Length >= 2 && data[^2] == '\r' && data[^1] == '\n')
        {
            // Valid end markers
        }

        // For TK103, we'll do a basic validation
        // In a real implementation, this would calculate the checksum properly
        // For now, we'll just ensure the frame has the expected structure
        var frameStr = System.Text.Encoding.ASCII.GetString(data).Trim();
        if (frameStr.Length < 30)
        {
            return false;
        }

        // Check for expected format: (IMEI),timestamp,lat,NS,lon,EW,speed,course
        var parts = frameStr.Split(',');
        if (parts.Length < 8)
        {
            return false;
        }

        // Validate IMEI format (15 digits)
        string imei = parts[0].Trim('(', ')');
        if (imei.Length != 15 || !imei.All(char.IsDigit))
        {
            return false;
        }

        return true;
    }

    private string ExtractDeviceId(GpsFrame frame)
    {
        return frame.Protocol switch
        {
            ProtocolType.GT06 => ExtractGT06DeviceId(frame),
            ProtocolType.H02 => ExtractH02DeviceId(frame),
            ProtocolType.TK103 => ExtractTK103DeviceId(frame),
            _ => "unknown"
        };
    }

    private string ExtractGT06DeviceId(GpsFrame frame)
    {
        if (frame.RawData.Length >= 9)
            return System.Text.Encoding.ASCII.GetString(frame.RawData, 4, 5).Trim('\0');
        return "unknown";
    }

    private string ExtractH02DeviceId(GpsFrame frame)
    {
        var frameStr = System.Text.Encoding.ASCII.GetString(frame.RawData);
        var parts = frameStr.Split(',');
        if (frameStr.StartsWith(ProtocolConstants.H02_HQ_START_MARKER))
            return parts.Length > 1 ? parts[1] : "unknown";
        return parts.Length > 0 ? parts[0] : "unknown";
    }

    private string ExtractTK103DeviceId(GpsFrame frame)
    {
        var frameStr = System.Text.Encoding.ASCII.GetString(frame.RawData);
        var parts = frameStr.Split(',');
        return parts.Length > 0 ? parts[0].Trim('(', ')') : "unknown";
    }

    private DateTime ExtractTimestamp(byte[] data, int offset)
    {
        var year = 2000 + data[offset];
        var month = data[offset + 1];
        var day = data[offset + 2];
        var hour = data[offset + 3];
        var minute = data[offset + 4];
        var second = data[offset + 5];
        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }

    private double ExtractCoordinate(byte[] data, int offset, bool isLatitude, byte statusByte)
    {
        var raw = (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
        var degrees = raw / 1800000.0; // Hotfix: Corrected GT06 coordinate conversion factor from 1/500s to decimal degrees.

        if (isLatitude)
        {
            // Bit 2: Latitude Hemisphere (0: South, 1: North)
            bool isNorth = (statusByte & 0b00000100) != 0;
            if (!isNorth) // If not North, it's South
                degrees = -degrees;
        }
        else // Longitude
        {
            // Bit 3: Longitude Hemisphere (0: East, 1: West)
            bool isWest = (statusByte & 0b00001000) != 0;
            if (isWest)
                degrees = -degrees;
        }

        return degrees;
    }

    private double ExtractSpeed(byte[] data, int offset)
    {
        var speed = (ushort)((data[offset] << 8) | data[offset + 1]);
        return speed * 1.852; // Convert knots to km/h
    }

    private double ExtractBearing(byte[] data, int offset)
    {
        return (ushort)((data[offset] << 8) | data[offset + 1]) / 100.0;
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
