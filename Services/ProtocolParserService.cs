// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Service for parsing raw GPS protocol frames into structured location data.
/// </summary>
public interface IProtocolParserService
{
    Task<LocationData> ParseFrameAsync(GpsFrame frame);
    Task<ProtocolType> DetectProtocolAsync(byte[] rawData);
    Task<bool> ValidateFrameAsync(GpsFrame frame);
}

/// <summary>
/// Implementation of protocol parser service.
/// </summary>
public class ProtocolParserService : IProtocolParserService
{
    /// <summary>
    /// Parses a GPS frame into location data based on protocol type.
    /// </summary>
    public async Task<LocationData> ParseFrameAsync(GpsFrame frame)
    {
        if (!frame.IsValid())
            throw new ParseException("Frame validation failed", frame.ToHex(), frame.Protocol);

        return frame.Protocol switch
        {
            ProtocolType.GT06 => ParseGT06Frame(frame),
            ProtocolType.H02 => ParseH02Frame(frame),
            ProtocolType.TK103 => ParseTK103Frame(frame),
            _ => throw new ParseException("Unsupported protocol", frame.ToHex())
        };
    }

    /// <summary>
    /// Detects protocol type from raw data.
    /// </summary>
    public async Task<ProtocolType> DetectProtocolAsync(byte[] rawData)
    {
        if (rawData.Length == 0)
            throw new ArgumentException("Raw data is empty");

        // GT06 protocol starts with 0x78
        if (rawData[0] == ProtocolConstants.GT06_START_MARKER)
            return ProtocolType.GT06;

        // TK103 protocol starts with 0x28
        if (rawData[0] == ProtocolConstants.TK103_START_MARKER)
            return ProtocolType.TK103;

        // H02 protocol starts with $GPRMC
        var header = System.Text.Encoding.ASCII.GetString(rawData.Take(6).ToArray());
        if (header.StartsWith(ProtocolConstants.H02_START_MARKER))
            return ProtocolType.H02;

        return ProtocolType.Unknown;
    }

    /// <summary>
    /// Validates frame structure and checksum.
    /// </summary>
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
        try
        {
            // GT06 frame structure: [78] [78] [length] [protocol_number] [data] [checksum] [0D] [0A]
            var data = frame.RawData;
            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frame),
                Protocol = ProtocolType.GT06,
                Timestamp = frame.ReceivedAt
            };

            // Extract GPS data from specific byte positions
            if (data.Length >= 30)
            {
                location.Timestamp = ExtractTimestamp(data, 5);
                location.Latitude = ExtractCoordinate(data, 11, true);
                location.Longitude = ExtractCoordinate(data, 15, false);
                location.Speed = ExtractSpeed(data, 19);
                location.Bearing = ExtractBearing(data, 21);
                location.SatelliteCount = data[23];
            }

            if (!location.IsValid())
                throw new ValidationException("Location data validation failed");

            return location;
        }
        catch (Exception ex)
        {
            throw new ParseException($"GT06 parsing failed: {ex.Message}", frame.ToHex(), ProtocolType.GT06);
        }
    }

    private LocationData ParseH02Frame(GpsFrame frame)
    {
        try
        {
            var frameStr = System.Text.Encoding.ASCII.GetString(frame.RawData).Trim();
            var parts = frameStr.Split(',');

            var location = new LocationData
            {
                DeviceId = ExtractDeviceId(frame),
                Protocol = ProtocolType.H02,
                Timestamp = frame.ReceivedAt
            };

            if (parts.Length >= 9)
            {
                location.Timestamp = DateTime.ParseExact(parts[1] + parts[2], "ddMMyyHHmmss", null);
                location.Latitude = ParseCoordinate(parts[3], parts[4]);
                location.Longitude = ParseCoordinate(parts[5], parts[6]);
                location.Speed = double.Parse(parts[7]);
                location.Bearing = double.Parse(parts[8]);
                location.SatelliteCount = int.Parse(parts[9]);
            }

            if (!location.IsValid())
                throw new ValidationException("Location data validation failed");

            return location;
        }
        catch (Exception ex)
        {
            throw new ParseException($"H02 parsing failed: {ex.Message}", frame.ToHex(), ProtocolType.H02);
        }
    }

    private LocationData ParseTK103Frame(GpsFrame frame)
    {
        try
        {
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
        catch (Exception ex)
        {
            throw new ParseException($"TK103 parsing failed: {ex.Message}", frame.ToHex(), ProtocolType.TK103);
        }
    }

    private bool ValidateGT06Checksum(GpsFrame frame)
    {
        if (frame.RawData.Length < 3)
            return false;

        var data = frame.RawData;
        var length = (data[2] << 8) | data[3];
        if (length + 5 > data.Length)
            return false;

        var checksum = 0;
        for (int i = 2; i < length + 3; i++)
            checksum ^= data[i];

        return checksum == data[length + 3];
    }

    private bool ValidateH02Checksum(GpsFrame frame) => true; // H02 typically doesn't use checksum
    private bool ValidateTK103Checksum(GpsFrame frame) => true; // TK103 validation can be protocol-specific

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
        return parts.Length > 0 ? parts[0] : "unknown";
    }

    private string ExtractTK103DeviceId(GpsFrame frame)
    {
        var frameStr = System.Text.Encoding.ASCII.GetString(frame.RawData);
        var parts = frameStr.Split(',');
        return parts.Length > 0 ? parts[0] : "unknown";
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

    private double ExtractCoordinate(byte[] data, int offset, bool isLatitude)
    {
        var raw = (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
        var degrees = raw / 30000000.0;
        return isLatitude ? degrees : degrees;
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
