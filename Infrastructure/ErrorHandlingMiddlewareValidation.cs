#nullable enable

namespace GpsTrackerProtocol.Infrastructure;

/// <summary>
/// Provides validation extension methods for <see cref="ErrorResponse"/> objects used in error handling middleware.
/// </summary>
public static class ErrorHandlingMiddlewareValidation
{
    /// <summary>
    /// Validates an <see cref="ErrorResponse"/> object and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The <see cref="ErrorResponse"/> to validate.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ErrorResponse? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.CorrelationId))
        {
            problems.Add("CorrelationId cannot be null or whitespace");
        }

        if (value.Timestamp == default)
        {
            problems.Add("Timestamp must be a valid DateTime, cannot be default");
        }
        else if (value.Timestamp.Kind != DateTimeKind.Utc)
        {
            problems.Add("Timestamp must be in UTC kind");
        }

        return problems;
    }

    /// <summary>
    /// Checks if an <see cref="ErrorResponse"/> object is valid.
    /// </summary>
    /// <param name="value">The <see cref="ErrorResponse"/> to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ErrorResponse? value) => value is not null && Validate(value).Count == 0;

    /// <summary>
    /// Ensures that an <see cref="ErrorResponse"/> object is valid, throwing <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The <see cref="ErrorResponse"/> to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this ErrorResponse? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorResponse validation failed: {string.Join("; ", problems)}");
        }
    }
}