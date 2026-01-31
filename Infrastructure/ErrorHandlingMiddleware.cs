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

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(Func<Task> next)
    {
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

    public ErrorResponse CreateErrorResponse(Exception ex, string correlationId)
    {
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
}
