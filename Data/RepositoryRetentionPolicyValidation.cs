#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using GpsTrackerProtocol.Configuration;

/// <summary>
/// Validates <see cref="RepositoryRetentionPolicy"/> instances before an in-memory repository adopts them.
/// </summary>
public static class RepositoryRetentionPolicyValidation
{
    /// <summary>
    /// Validates a retention policy. A null policy means unlimited retention and is accepted.
    /// Zero for <c>MaxPointsPerDevice</c> or <c>MaxAge</c> disables that limit; negative values are rejected
    /// rather than silently treated as disabled.
    /// </summary>
    /// <param name="retentionPolicy">The policy to validate. May be null.</param>
    /// <returns>The same policy instance when valid, or null when <paramref name="retentionPolicy"/> is null.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>MaxPointsPerDevice</c> or <c>MaxAge</c> is negative.</exception>
    public static RepositoryRetentionPolicy? EnsureValid(RepositoryRetentionPolicy? retentionPolicy)
    {
        if (retentionPolicy is null)
        {
            return null;
        }

        if (retentionPolicy.MaxPointsPerDevice is { } maxPoints)
        {
            ConfigurationGuards.RequireNonNegative(maxPoints, "retentionPolicy.MaxPointsPerDevice");
        }

        if (retentionPolicy.MaxAge is { } maxAge)
        {
            ConfigurationGuards.RequireNonNegative(maxAge, "retentionPolicy.MaxAge");
        }

        return retentionPolicy;
    }
}
