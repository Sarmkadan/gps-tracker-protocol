#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Infrastructure;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;

/// <summary>
/// Global error handling middleware that catches and logs exceptions uniformly.
/// Maps domain exceptions to appropriate error responses with context.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record handled and unhandled exceptions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Runs the supplied delegate and translates exceptions it throws into logged, wrapped errors.
    /// <see cref="ParseException"/> and <see cref="ValidationException"/> are logged as warnings and
    /// rethrown as <see cref="ApplicationException"/>. <see cref="OperationCanceledException"/> is logged
    /// as information and rethrown unchanged. Any other exception is logged as an error and rethrown
    /// as <see cref="ApplicationException"/>.
    /// </summary>
    /// <param name="next">The delegate to execute.</param>
    /// <returns>A task that completes when <paramref name="next"/> completes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is null.</exception>
    /// <exception cref="ApplicationException">Thrown when <paramref name="next"/> fails with an exception other than cancellation.</exception>
    public async Task InvokeAsync(Func<Task> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        try
        {
            await next();
        }
        catch (ParseException ex)
        {
            _logger.LogWarning(ex, "Parse error: {Protocol}, Raw: {RawData}",
                ex.Protocol, ex.RawData);
            throw new ApplicationException($"Failed to parse {ex.Protocol} frame", ex);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed: {Message}", ex.Message);
            throw new ApplicationException($"Validation error: {ex.Message}", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            throw new ApplicationException("An unexpected error occurred", ex);
        }
    }

    /// <summary>
    /// Builds an <see cref="ErrorResponse"/> for an exception, choosing an error code from its type.
    /// </summary>
    /// <param name="ex">The exception to describe.</param>
    /// <param name="correlationId">Identifier used to correlate the response with the originating request.</param>
    /// <returns>An error response containing the error code, message, correlation ID, UTC timestamp and stack trace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is null or empty.</exception>
    public ErrorResponse CreateErrorResponse(Exception ex, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrEmpty(correlationId);
        var errorCode = ex switch
        {
            ParseException => "PARSE_ERROR",
            ValidationException => "VALIDATION_ERROR",
            ArgumentException => "INVALID_ARGUMENT",
            _ => "INTERNAL_ERROR"
        };

        return new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = ex.Message,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            StackTrace = ex.StackTrace
        };
    }
}

public record ErrorResponse
{
    public string ErrorCode { get; set; }
    public string Message { get; set; }
    public string CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public string StackTrace { get; set; }

    public override string ToString() => $"ErrorHandlingMiddleware {{ ErrorCode = {ErrorCode}, Message = {Message}, CorrelationId = {CorrelationId}, Timestamp = {Timestamp}, StackTrace = {StackTrace} }}";
}
