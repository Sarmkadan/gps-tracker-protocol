#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

using System.Numerics;

/// <summary>
/// Shared guard clauses for configuration values. Every failure is an
/// <see cref="ArgumentOutOfRangeException"/> with the same message shape, so a bad setting
/// is reported identically no matter which component it configures.
/// </summary>
internal static class ConfigurationGuards
{
    /// <summary>
    /// Ensures a value is strictly greater than zero. NaN is rejected as well.
    /// </summary>
    /// <typeparam name="T">The numeric type of the setting.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the setting, reported in the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paramName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paramName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not greater than zero.</exception>
    public static void RequirePositive<T>(T value, string paramName) where T : INumber<T>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        if (!(value > T.Zero))
        {
            throw CreateFailure(paramName, value, "must be greater than 0");
        }
    }

    /// <summary>
    /// Ensures a value is zero or greater. NaN is rejected as well.
    /// </summary>
    /// <typeparam name="T">The numeric type of the setting.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the setting, reported in the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paramName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paramName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static void RequireNonNegative<T>(T value, string paramName) where T : INumber<T>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        if (!(value >= T.Zero))
        {
            throw CreateFailure(paramName, value, "must be greater than or equal to 0");
        }
    }

    /// <summary>
    /// Ensures a time span is zero or greater.
    /// </summary>
    /// <param name="value">The time span to check.</param>
    /// <param name="paramName">The name of the setting, reported in the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paramName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paramName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static void RequireNonNegative(TimeSpan value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        if (value < TimeSpan.Zero)
        {
            throw CreateFailure(paramName, value, "must be greater than or equal to 0");
        }
    }

    /// <summary>
    /// Ensures a floating-point value is finite (neither NaN nor infinite).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the setting, reported in the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paramName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paramName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is NaN or infinite.</exception>
    public static void RequireFinite(double value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        if (!double.IsFinite(value))
        {
            throw CreateFailure(paramName, value, "must be a finite number");
        }
    }

    private static ArgumentOutOfRangeException CreateFailure(string paramName, object actual, string rule) =>
        new(paramName, actual, $"{paramName} {rule} (actual: {actual}).");
}
