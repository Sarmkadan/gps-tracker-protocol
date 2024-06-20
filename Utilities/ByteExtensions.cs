// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Extension methods for byte array operations.
/// Provides utilities for parsing binary GPS protocol frames.
/// </summary>
public static class ByteExtensions
{
    /// <summary>
    /// Converts byte array to hexadecimal string representation.
    /// </summary>
    public static string ToHexString(this byte[] data, bool addSpaces = false)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        var hex = BitConverter.ToString(data);
        return addSpaces ? hex : hex.Replace("-", "");
    }

    /// <summary>
    /// Extracts a 16-bit unsigned integer from bytes in big-endian format.
    /// </summary>
    public static ushort ToUInt16BigEndian(this byte[] data, int offset)
    {
        if (data == null || offset + 1 >= data.Length)
            throw new ArgumentException("Invalid offset or data length");

        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    /// <summary>
    /// Extracts a 32-bit unsigned integer from bytes in big-endian format.
    /// </summary>
    public static uint ToUInt32BigEndian(this byte[] data, int offset)
    {
        if (data == null || offset + 3 >= data.Length)
            throw new ArgumentException("Invalid offset or data length");

        return (uint)((data[offset] << 24) | (data[offset + 1] << 16) |
                      (data[offset + 2] << 8) | data[offset + 3]);
    }

    /// <summary>
    /// Calculates XOR checksum for protocol validation.
    /// </summary>
    public static byte CalculateXorChecksum(this byte[] data, int startIndex, int length)
    {
        if (data == null || startIndex < 0 || length <= 0)
            return 0;

        byte checksum = 0;
        for (int i = startIndex; i < startIndex + length && i < data.Length; i++)
            checksum ^= data[i];

        return checksum;
    }

    /// <summary>
    /// Extracts substring from byte array and converts to ASCII string.
    /// </summary>
    public static string ToAsciiString(this byte[] data, int startIndex, int length)
    {
        if (data == null || startIndex < 0 || length <= 0)
            return string.Empty;

        try
        {
            return System.Text.Encoding.ASCII.GetString(data, startIndex,
                Math.Min(length, data.Length - startIndex));
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if byte array starts with specified marker bytes.
    /// </summary>
    public static bool StartsWithMarker(this byte[] data, params byte[] marker)
    {
        if (data == null || marker == null || data.Length < marker.Length)
            return false;

        for (int i = 0; i < marker.Length; i++)
        {
            if (data[i] != marker[i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Finds the index of specified byte sequence in array.
    /// </summary>
    public static int IndexOfSequence(this byte[] data, byte[] sequence)
    {
        if (data == null || sequence == null || sequence.Length == 0)
            return -1;

        for (int i = 0; i <= data.Length - sequence.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < sequence.Length; j++)
            {
                if (data[i + j] != sequence[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Copies a range from byte array and returns a new array.
    /// </summary>
    public static byte[] CopyRange(this byte[] data, int startIndex, int length)
    {
        if (data == null || startIndex < 0 || length <= 0)
            return Array.Empty<byte>();

        var result = new byte[Math.Min(length, data.Length - startIndex)];
        Array.Copy(data, startIndex, result, 0, result.Length);
        return result;
    }
}
