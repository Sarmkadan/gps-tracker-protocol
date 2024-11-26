#nullable enable
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
    static ByteExtensions()
    {
        // Static constructor for any initialization
    }

    /// <summary>
    /// Converts byte array to hexadecimal string representation.
    /// </summary>
    /// <param name="data">The byte array to convert.</param>
    /// <param name="addSpaces">Whether to include spaces between each byte.</param>
    /// <returns>Hexadecimal string representation of the byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public static string ToHexString(this byte[] data, bool addSpaces = false)
    {
        ArgumentNullException.ThrowIfNull(data);

        return data.Length == 0
            ? string.Empty
            : addSpaces
                ? BitConverter.ToString(data)
                : BitConverter.ToString(data).Replace("-", "");
    }

    /// <summary>
    /// Extracts a 16-bit unsigned integer from bytes in big-endian format.
    /// </summary>
    /// <param name="data">The byte array containing the data.</param>
    /// <param name="offset">The zero-based index of the first byte.</param>
    /// <returns>16-bit unsigned integer value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="offset"/> is negative or when there are not enough bytes from the offset.</exception>
    public static ushort ToUInt16BigEndian(this byte[] data, int offset)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset + 1, data.Length);

        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    /// <summary>
    /// Extracts a 32-bit unsigned integer from bytes in big-endian format.
    /// </summary>
    /// <param name="data">The byte array containing the data.</param>
    /// <param name="offset">The zero-based index of the first byte.</param>
    /// <returns>32-bit unsigned integer value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="offset"/> is negative or when there are not enough bytes from the offset.</exception>
    public static uint ToUInt32BigEndian(this byte[] data, int offset)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset + 3, data.Length);

        return (uint)((data[offset] << 24) | (data[offset + 1] << 16) |
        (data[offset + 2] << 8) | data[offset + 3]);
    }

    /// <summary>
    /// Calculates XOR checksum for protocol validation.
    /// </summary>
    /// <param name="data">The byte array to calculate checksum for.</param>
    /// <param name="startIndex">The zero-based starting index.</param>
    /// <param name="length">The number of bytes to include in the checksum.</param>
    /// <returns>XOR checksum byte value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> is negative or <paramref name="length"/> is not positive.</exception>
    public static byte CalculateXorChecksum(this byte[] data, int startIndex, int length)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(length, 0);

        byte checksum = 0;
        int endIndex = Math.Min(startIndex + length, data.Length);
        for (int i = startIndex; i < endIndex; i++)
            checksum ^= data[i];

        return checksum;
    }

    /// <summary>
    /// Extracts substring from byte array and converts to ASCII string.
    /// </summary>
    /// <param name="data">The byte array containing the ASCII data.</param>
    /// <param name="startIndex">The zero-based starting index.</param>
    /// <param name="length">The number of bytes to convert.</param>
    /// <returns>ASCII string representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> is negative or <paramref name="length"/> is not positive.</exception>
    public static string ToAsciiString(this byte[] data, int startIndex, int length)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(length, 0);

        return System.Text.Encoding.ASCII.GetString(data, startIndex,
            Math.Min(length, data.Length - startIndex));
    }

    /// <summary>
    /// Checks if byte array starts with specified marker bytes.
    /// </summary>
    /// <param name="data">The byte array to check.</param>
    /// <param name="marker">The marker bytes to search for at the beginning.</param>
    /// <returns>True if the array starts with the marker; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> or <paramref name="marker"/> is null.</exception>
    public static bool StartsWithMarker(this byte[] data, params byte[] marker)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(marker);
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, marker.Length);

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
    /// <param name="data">The byte array to search in.</param>
    /// <param name="sequence">The byte sequence to find.</param>
    /// <returns>The zero-based index of the first occurrence, or -1 if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> or <paramref name="sequence"/> is null.</exception>
    public static int IndexOfSequence(this byte[] data, byte[] sequence)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(sequence);

        if (sequence.Length == 0)
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
    /// <param name="data">The source byte array.</param>
    /// <param name="startIndex">The zero-based starting index.</param>
    /// <param name="length">The number of bytes to copy.</param>
    /// <returns>A new byte array containing the copied range.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> is negative or <paramref name="length"/> is not positive.</exception>
    public static byte[] CopyRange(this byte[] data, int startIndex, int length)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(length, 0);

        var result = new byte[Math.Min(length, data.Length - startIndex)];
        Array.Copy(data, startIndex, result, 0, result.Length);
        return result;
    }
}