#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

using System.Collections.Generic;

/// <summary>
/// Provides validation methods for byte arrays intended for use with <see cref="ByteExtensions"/> extension methods.
/// Ensures byte arrays meet the requirements of protocol parsing and checksum validation operations.
/// </summary>
public static class ByteExtensionsValidation
{
    /// <summary>
    /// Validates a byte array for use with <see cref="ByteExtensions"/> methods and returns human-readable problems.
    /// </summary>
    /// <param name="value">The byte array to validate.</param>
    /// <returns>
    /// A read-only list of validation error messages, or an empty list if the value is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Length == 0
            ? new[] { "Byte array cannot be empty" }
            : Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a byte array is valid for use with <see cref="ByteExtensions"/> methods.
    /// </summary>
    /// <param name="value">The byte array to validate.</param>
    /// <returns>true if the byte array is valid; otherwise, false.</returns>
    public static bool IsValid(this byte[] value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a byte array is valid for use with <see cref="ByteExtensions"/> methods.
    /// Throws an <see cref="ArgumentException"/> listing the problems if validation fails.
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