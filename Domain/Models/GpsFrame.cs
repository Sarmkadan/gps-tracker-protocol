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
        if (RawData.Length == 0 || !IsValidChecksum)
            return false;

        return Protocol switch
        {
            ProtocolType.GT06 => RawData.Length >= 15,
            ProtocolType.H02 => RawData.Length >= 32,
            ProtocolType.TK103 => RawData.Length >= 30,
            _ => false
        };
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
