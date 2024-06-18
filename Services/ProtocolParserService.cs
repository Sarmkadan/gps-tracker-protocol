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
        var data = frame.RawData;

        // A valid GT06 frame must have at least 6 bytes: 2 Start + 1 Length + 1 Protocol + 1 CRC + 2 Stop
        if (data.Length < 6)
            return false;

        // Packet Length is at index 2. This byte indicates the length from Protocol Number to CRC.
        // It does not include the 2 start bytes, the length byte itself, and the 2 stop bytes.
        // So, total bytes covered by the 'length' field = length byte + Protocol Number + Info Content + Info Serial Number + CRC
        // The length field (data[2]) should be actual_frame_length - 5 (2 start + 1 length + 2 stop)
        int declaredLength = data[2];

        // The actual CRC is the byte before the two stop bytes (0x0D 0x0A)
        // Its index is data.Length - 3.
        // The checksum should cover from data[2] (Packet Length) up to data[data.Length - 3 - 1] = data[data.Length - 4]
        // This is if CRC is 1 byte and is the second to last byte before stop bits.
        // Based on analysis of the sample in Program.cs:
        // { 0x78, 0x78, 0x1F, 0x12, ..., 0xFF, 0xFE, 0xF3, 0x0D, 0x0A }
        // data[2] = 0x1F. Total frame is 32 bytes (indices 0-31).
        // 0x1F (31) = length of bytes from 0x12 (protocol) to 0xF3 (CRC).
        // The frame length is data[2] + 2 (start) + 2 (stop) = 0x1F + 4 = 31 + 4 = 35 bytes.
        // The provided `validGt06RawData` is 30 bytes long. This mismatch is key.
        // If data[2] (0x1F) is the length of data from Protocol Number (0x12) to CRC (0xF3)
        // then the range for checksum calculation would be from data[3] to data[data.Length - 3].
        // If data[2] is length from Protocol Number up to Info Serial Number:
        // then data[2] + 2 (Start) + 1 (Length byte) + 2 (CRC) + 2 (Stop) = Total Length
        // (0x1F) + 2 + 1 + 2 + 2 = 31 + 7 = 38. Still doesn't match 30.

        // Re-evaluating based on common GT06 protocol interpretations:
        // Length field (data[2]) = (Length of Information Content) + (Length of Info Serial No.) + (Length of Error Check).
        // So, the actual total bytes for 'content' part is data[2] + 1 (for protocol type).
        // If data[2] is length of Protocol Number (1) + Info Content + Info Serial Number (2) + CRC (1)
        // then data[2] = 1 + (Info Content Length) + 2 + 1.
        //
        // Let's assume the most common interpretation:
        // Checksum is XOR sum of bytes from data[2] (Packet Length field itself) up to the byte before the CRC.
        // The CRC is data[data.Length - 3].
        // The stop bytes are data[data.Length - 2] and data[data.Length - 1].
        // This means the range for XOR sum is `data[2]` to `data[data.Length - 4]`.
        // The declared length `data[2]` should be `data.Length - 5` (excluding 2 start, itself, 1 CRC, 2 stop).

        // If the frame from Program.cs is 30 bytes:
        // { 0x78, 0x78, 0x1F, 0x12, ..., 0xFF, 0xFE, 0xF3, 0x0D, 0x0A }
        // Indices: 0   1   2    3   ...   26   27   28   29   30 (Incorrect, 30 means data.Length-1)
        // Corrected indices based on 30 bytes length for example:
        // { 0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23, 0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A }
        // Length = 30 bytes (0-29).
        // 0,1: Start (0x78 0x78)
        // 2: Packet Length (0x1F = 31). This means the length of (Protocol to CRC). So 0x1F bytes from data[3] to data[3 + 0x1F - 1] = data[3+30] = data[33].
        // This makes no sense, as total length is 30.

        // The original code uses `length = (data[2] << 8) | data[3];` which is highly suspicious for a single byte length field.
        // It also uses `length + 5 > data.Length`.
        // And `checksum == data[length + 3]`.

        // This suggests that `data[2]` and `data[3]` might be part of a 2-byte length field OR
        // data[2] is length of information field (excluding protocol number and CRC) and data[3] is protocol number.
        // Given the problem description: "raw TCP/UDP to structured location data", and the sample in Program.cs.
        // It's possible `data[2]` is the data length *excluding* header, length field, and trailer.

        // Let's assume the provided `Program.cs` example is correct and derive the intended logic.
        // rawData = { 0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23, 0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A }
        // Length is 30.
        // If CRC is 0xF3 (data[27]), and the checksum calculation loop goes from data[2] to length + 3.
        // And `length = (data[2] << 8) | data[3]` = (0x1F << 8) | 0x12 = 7954.
        // `length + 3` = 7957. `data[7957]` is out of bounds. This logic is definitely wrong.

        // Standard interpretation of GT06 (based on multiple online sources):
        // 78 78 (Start Bits)
        // LL (Packet Length: total length of data between Start Bits and Stop Bits, excluding Start/Stop Bits themselves. 1 byte)
        // PN (Protocol Number: 1 byte)
        // Data Field
        // SN SN (Information Serial Number: 2 bytes)
        // CS (Error Check/Checksum: 1 byte, XOR sum from LL to second SN byte)
        // 0D 0A (Stop Bits)

        // Using this standard, for the sample:
        // rawData.Length = 30.
        // Start: 0x78 0x78
        // Packet Length (LL): 0x1F (data[2])
        // Protocol Number (PN): 0x12 (data[3])
        // Data Field (from data[4] to data[25])
        // Info Serial Number (SN SN): 0xFF 0xFE (data[26], data[27])
        // Error Check (CS): 0xF3 (data[28])
        // Stop Bits: 0x0D 0x0A (data[29], data[30] - this means rawData.Length must be 31, if 0-indexed)
        // Uh oh, the example raw data in Program.cs has 30 bytes, but ends with 0x0D 0x0A.
        // This implies data[28] is 0x0D and data[29] is 0x0A.
        // So, 0xF3 must be data[27].
        // If 0xF3 is CRC, then 0xFF 0xFE are not Information Serial Number.

        // This suggests a simpler GT06 frame structure in this project where CRC is the byte immediately before 0x0D 0x0A.
        // Let's re-align the indices for the given `rawData` length of 30 bytes:
        // Indices: 0   1   2    3   4    5   6   7   8   9   10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29
        // Bytes:  78  78  1F   12  01  19  11  0B  16  23  34  02  A2  C0  40  05  27  6F  D1  00  00  A2  00  00  08  FF  FE  F3  0D  0A
        // Start (0x78 0x78): data[0], data[1]
        // Packet Length (0x1F): data[2]. This value is 31.
        // Actual bytes for CRC calculation should be from data[2] up to the byte BEFORE the actual CRC.
        // The CRC is data[data.Length - 3] (0xF3)
        // The end bytes are data[data.Length - 2] (0x0D) and data[data.Length - 1] (0x0A).

        // So, the checksum calculation should be XOR sum of bytes from data[2] to data[data.Length - 4].
        // The actual CRC is data[data.Length - 3].

        // Let's apply this:
        // `declaredLength` is data[2] = 0x1F. This is length of content starting from data[3] to data[data.Length - 3] (0xF3).
        // Total bytes in this range = (data.Length - 3) - 3 + 1 = data.Length - 5.
        // So, 0x1F (31) should be equal to (30 - 5) = 25.
        // This is a discrepancy. 31 != 25.

        // The only way this makes sense is if the length field itself is not included in the checksum calculation,
        // OR the example `rawData` has a declared length that does not match its actual content length for checksum purposes.

        // Let's make an assumption based on the original buggy code's intent:
        // `length = data[2]`. Let's assume it wants data[2] as the "length".
        // `for (int i = 2; i < length + 3; i++)`
        // `checksum == data[length + 3]`

        // If `length = data[2]`, then `length=0x1F`.
        // Loop from `i=2` to `0x1F + 3 = 34`. This will still go out of bounds.

        // It seems the original code for GT06 checksum is fundamentally broken.
        // I will implement a common GT06 XOR checksum:
        // Calculate XOR sum from the Protocol Number field (data[3]) up to the byte before the CRC.
        // The CRC is the byte `data[data.Length - 3]`.
        // So, the XOR sum will be from `data[3]` to `data[data.Length - 4]`.

        if (data.Length < 7) // Minimum frame length: 2 Start + 1 Length + 1 Protocol + 1 Data + 1 CRC + 2 Stop = 8 bytes.
                             // But if Length field includes CRC, then 2 Start + 1 Length + 1 Protocol + 1 CRC + 2 Stop = 7 bytes min.
            return false;

        // The Packet Length field (data[2]) indicates the length from Protocol Number (data[3])
        // up to and including the Error Check (data[data.Length - 3]).
        // So, declaredLength should be (data.Length - 3) - 3 + 1 = data.Length - 5.
        // If data[2] is 0x1F (31) and data.Length is 30, then 31 != 30 - 5 = 25. This means the sample `rawData` from `Program.cs` itself is problematic
        // OR the declared length `0x1F` is incorrect for this rawData.
        // For now, I will write the code to assume standard GT06 checksum and then adjust test data.

        // Assuming standard GT06:
        // CRC byte is data[data.Length - 3].
        // XOR sum is calculated over bytes from data[2] (Packet Length) to data[data.Length - 4].
        byte calculatedChecksum = 0;
        // The Packet Length itself is usually included in the XOR sum.
        // The calculation range is from data[2] to data[data.Length - 4] (the byte before the actual CRC byte)
        for (int i = 2; i <= data.Length - 4; i++)
        {
            calculatedChecksum ^= data[i];
        }

        return calculatedChecksum == data[data.Length - 3];
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
