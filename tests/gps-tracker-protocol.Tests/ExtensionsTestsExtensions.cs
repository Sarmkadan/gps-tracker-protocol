#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpsTrackerProtocol.Utilities;

namespace GpsTrackerProtocol.Tests;

public static class ExtensionsTestsExtensions
{
    /// <summary>
    /// Converts byte array to hexadecimal string with configurable formatting.
    /// </summary>
    /// <param name="data">The byte array to convert.</param>
    /// <param name="addSpaces">Whether to add spaces between each byte.</param>
    /// <param name="addDashes">Whether to add dashes between each byte.</param>
    /// <returns>Hexadecimal string representation of the byte array.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static string ToHexString(this byte[] data, bool addSpaces = false, bool addDashes = false)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0)
            return string.Empty;

        var hex = BitConverter.ToString(data);

        if (addSpaces)
            return hex;

        if (addDashes)
            return hex;

        return hex.Replace("-", "");
    }

    /// <summary>
    /// Converts byte array to hexadecimal string with uppercase formatting.
    /// </summary>
    /// <param name="data">The byte array to convert.</param>
    /// <param name="addSpaces">Whether to add spaces between each byte.</param>
    /// <returns>Hexadecimal string representation of the byte array in uppercase.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static string ToHexStringUpper(this byte[] data, bool addSpaces = false)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0)
            return string.Empty;

        var hex = BitConverter.ToString(data).Replace("-", "");
        return addSpaces ? hex : hex;
    }

    /// <summary>
    /// Calculates XOR checksum for the entire byte array.
    /// </summary>
    /// <param name="data">The byte array to calculate checksum for.</param>
    /// <returns>The XOR checksum byte.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static byte CalculateXorChecksum(this byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0)
            return 0;

        byte checksum = 0;
        foreach (var b in data)
        {
            checksum ^= b;
        }
        return checksum;
    }

    /// <summary>
    /// Validates if byte array contains a specific byte sequence.
    /// </summary>
    /// <param name="data">The byte array to search in.</param>
    /// <param name="sequence">The byte sequence to search for.</param>
    /// <returns><see langword="true"/> if the sequence is found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> or <paramref name="sequence"/> is <see langword="null"/>.</exception>
    public static bool ContainsSequence(this byte[] data, byte[] sequence)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(sequence);

        if (sequence.Length == 0)
            return false;

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
                return true;
        }
        return false;
    }
}