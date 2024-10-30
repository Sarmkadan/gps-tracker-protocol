#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Validation helpers for byte array validation based on ByteExtensions capabilities.
/// Provides validation methods for byte arrays used with ByteExtensions methods.
/// </summary>
public static class ByteExtensionsValidation
{
    /// <summary>
    /// Validates a byte array for use with ByteExtensions methods and returns human-readable problems.
    /// </summary>
    /// <param name="value">The byte array to validate.</param>
    /// <returns>
    /// A read-only list of validation error messages, or an empty list if the value is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.Length == 0)
        {
            problems.Add("Byte array cannot be empty");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether a byte array is valid for use with ByteExtensions methods.
    /// </summary>
    /// <param name="value">The byte array to validate.</param>
    /// <returns>true if the byte array is valid; otherwise, false.</returns>
    public static bool IsValid(this byte[] value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a byte array is valid for use with ByteExtensions methods.
    /// Throws an ArgumentException listing the problems if validation fails.
    /// </summary>
    /// <param name="value">The byte array to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the byte array is invalid, listing all validation problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", problems), nameof(value));
        }
    }
}