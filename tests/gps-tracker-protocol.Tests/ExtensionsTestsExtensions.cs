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
    public static string ToHexString(this byte[] data, bool addSpaces = false, bool addDashes = false)
    {
        if (data is null || data.Length == 0)
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
    public static string ToHexStringUpper(this byte[] data, bool addSpaces = false)
    {
        if (data is null || data.Length == 0)
            return string.Empty;

        var hex = BitConverter.ToString(data).Replace("-", "");
        return addSpaces ? hex : hex;
    }

    /// <summary>
    /// Calculates XOR checksum for the entire byte array.
    /// </summary>
    public static byte CalculateXorChecksum(this byte[] data)
    {
        if (data is null || data.Length == 0)
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
    public static bool ContainsSequence(this byte[] data, byte[] sequence)
    {
        if (data is null || sequence is null || sequence.Length == 0)
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
