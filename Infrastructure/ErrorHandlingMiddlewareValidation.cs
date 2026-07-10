#nullable enable

namespace GpsTrackerProtocol.Infrastructure;

/// <summary>
/// Validation helpers for error handling middleware components.
/// </summary>
public static class ErrorHandlingMiddlewareValidation
{
    /// <summary>
    /// Validates an ErrorResponse object and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The ErrorResponse to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this ErrorResponse value)
    {
        if (value == null)
        {
            return new[] { "ErrorResponse cannot be null" };
        }

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

        if (value.Timestamp.Kind != DateTimeKind.Utc)
        {
            problems.Add("Timestamp must be in UTC kind");
        }

        return problems;
    }

    /// <summary>
    /// Checks if an ErrorResponse object is valid.
    /// </summary>
    /// <param name="value">The ErrorResponse to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ErrorResponse value)
    {
        return value != null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an ErrorResponse object is valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="value">The ErrorResponse to validate</param>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValid(this ErrorResponse value)
    {
        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorResponse validation failed: {string.Join("; ", problems)}");
        }
    }
}