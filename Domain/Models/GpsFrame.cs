#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents a raw GPS protocol frame before parsing.
/// </summary>
public class GpsFrame
{
    public string FrameId { get; set; } = string.Empty;
    public ProtocolType Protocol { get; set; }
    public byte[] RawData { get; set; } = [];
    public DateTime ReceivedAt { get; set; }
    public string SourceAddress { get; set; } = string.Empty;
    public int SourcePort { get; set; }
    public bool IsValidChecksum { get; set; }
    public string? ChecksumValue { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// Validates frame structure and integrity.
    /// </summary>
    public bool IsValid()
    {
        if (RawData.Length == 0)
            return false;

        // Check minimum frame sizes
        if (Protocol == ProtocolType.GT06 && RawData.Length < 15)
            return false;

        if (Protocol == ProtocolType.H02 && RawData.Length < 32)
            return false;

        if (Protocol == ProtocolType.TK103 && RawData.Length < 30)
            return false;

        // Check checksum if it hasn't been validated yet
        if (!IsValidChecksum)
        {
            // Calculate checksum on-demand if not already validated
            IsValidChecksum = Protocol switch
            {
                ProtocolType.GT06 => ValidateGT06Checksum(this),
                ProtocolType.H02 => ValidateH02Checksum(this),
                ProtocolType.TK103 => ValidateTK103Checksum(this),
                _ => false
            };
        }

        return IsValidChecksum;
    }

    private bool ValidateGT06Checksum(GpsFrame frame)
    {
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
        // TK103 protocol uses simple checksum: XOR of all bytes
        var data = frame.RawData;

        // Minimum frame: (device_id),YYYYMMDDHHMMSS,lat,NS,lon,EW,speed,course\r\n
        if (data.Length < ProtocolConstants.TK103_MIN_FRAME_SIZE)
        {
            return false;
        }

        // TK103 frames typically end with CRLF
        if (data.Length >= 2 && data[^2] == '\r' && data[^1] == '\n')
        {
            // Valid end markers
        }

        // For TK103, calculate checksum: XOR of all bytes except CRLF
        byte calculatedChecksum = 0;
        int bytesToCheck = data.Length >= 2 && data[^2] == '\r' && data[^1] == '\n'
            ? data.Length - 2
            : data.Length;

        for (int i = 0; i < bytesToCheck; i++)
        {
            calculatedChecksum ^= data[i];
        }

        // TK103 checksum is typically not transmitted, so we just validate the format
        // In a real implementation, this would compare against a transmitted checksum
        return true;
    }

    /// <summary>
    /// Gets hex representation of raw data.
    /// </summary>
    public string ToHex()
    {
        return BitConverter.ToString(RawData).Replace("-", "");
    }

    /// <summary>
    /// Extracts a substring from raw data at specified byte offset.
    /// </summary>
    public byte[] ExtractBytes(int offset, int length)
    {
        if (offset < 0 || offset + length > RawData.Length)
            throw new ArgumentException("Invalid offset or length");

        var buffer = new byte[length];
        Array.Copy(RawData, offset, buffer, 0, length);
        return buffer;
    }

    /// <summary>
    /// Parses a value from raw data at specified offset with given length.
    /// </summary>
    public string ExtractString(int offset, int length, bool reverseBytes = false)
    {
        var bytes = ExtractBytes(offset, length);
        if (reverseBytes)
            Array.Reverse(bytes);
        return System.Text.Encoding.ASCII.GetString(bytes).Trim('\0');
    }

    public override string ToString() =>
        $"GpsFrame({Protocol}) - {RawData.Length} bytes - {ReceivedAt:O} from {SourceAddress}:{SourcePort}";
}
