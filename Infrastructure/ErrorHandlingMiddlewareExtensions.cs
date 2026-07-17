#nullable enable

namespace GpsTrackerProtocol.Infrastructure;

/// <summary>
/// Extension methods for <see cref="ErrorHandlingMiddleware"/> that provide additional functionality
/// for error handling and response creation.
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Creates an error response with the specified error code and message.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance.</param>
    /// <param name="errorCode">The error code to set in the response.</param>
    /// <param name="message">The error message to set in the response.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <returns>An <see cref="ErrorResponse"/> with the specified values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="errorCode"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="correlationId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ErrorResponse CreateErrorResponse(
        this ErrorHandlingMiddleware middleware,
        string errorCode,
        string message,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        return new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            StackTrace = null
        };
    }

    /// <summary>
    /// Creates an error response with the specified exception, correlation ID, and optional
    /// custom error code override.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="customErrorCode">Optional custom error code to override the default mapping.</param>
    /// <returns>An <see cref="ErrorResponse"/> with exception details.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="correlationId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ErrorResponse CreateErrorResponse(
        this ErrorHandlingMiddleware middleware,
        Exception ex,
        string correlationId,
        string? customErrorCode = null)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        var errorCode = customErrorCode ?? GetDefaultErrorCode(ex);
        var message = ex.Message;
        var stackTrace = ex.StackTrace;

        return new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            StackTrace = stackTrace
        };
    }

    /// <summary>
    /// Creates a standardized error response for validation failures.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance.</param>
    /// <param name="validationErrors">Collection of validation error messages.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <returns>An <see cref="ErrorResponse"/> with validation error details.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="validationErrors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="correlationId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ErrorResponse CreateValidationErrorResponse(
        this ErrorHandlingMiddleware middleware,
        IEnumerable<string> validationErrors,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(validationErrors);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        var errorMessages = string.Join("; ", validationErrors);

        return new ErrorResponse
        {
            ErrorCode = "VALIDATION_ERROR",
            Message = $"Validation failed: {errorMessages}",
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            StackTrace = null
        };
    }

    /// <summary>
    /// Creates a standardized error response for parse failures with additional context.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance.</param>
    /// <param name="protocol">The protocol name that failed to parse.</param>
    /// <param name="rawData">The raw data that caused the parse error.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <returns>An <see cref="ErrorResponse"/> with parse error details.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="protocol"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="rawData"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="correlationId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ErrorResponse CreateParseErrorResponse(
        this ErrorHandlingMiddleware middleware,
        string protocol,
        string rawData,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(protocol);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawData);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        return new ErrorResponse
        {
            ErrorCode = "PARSE_ERROR",
            Message = $"Failed to parse {protocol} frame",
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            StackTrace = $"Protocol: {protocol}\nRaw data: {rawData}"
        };
    }

    private static string GetDefaultErrorCode(Exception ex) => ex switch
    {
        ArgumentException => "ARGUMENT_ERROR",
        InvalidOperationException => "INVALID_OPERATION",
        KeyNotFoundException => "NOT_FOUND",
        UnauthorizedAccessException => "UNAUTHORIZED",
        _ => "INTERNAL_ERROR"
    };
}