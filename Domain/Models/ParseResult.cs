#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unified parse result type for GPS tracker protocol parsers
// =====================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents the result of a parsing operation with either success or failure.
/// </summary>
/// <typeparam name="T">The type of successfully parsed data.</typeparam>
public readonly record struct ParseResult<T> where T : class
{
    private readonly bool _isSuccess;
    private readonly T? _value;
    private readonly ParseError? _error;

    /// <summary>
    /// Indicates whether the parsing operation succeeded.
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// The successfully parsed value (only valid when IsSuccess is true).
    /// </summary>
    public T? Value => _isSuccess ? _value : throw new InvalidOperationException("Cannot access Value when parsing failed");

    /// <summary>
    /// Gets the error details if parsing failed.
    /// </summary>
    public ParseError? Error => _error;

    /// <summary>
    /// Gets the byte offset where parsing failed (only valid when IsSuccess is false).
    /// </summary>
    public int ErrorOffset => _error?.Offset ?? throw new InvalidOperationException("Cannot access ErrorOffset when parsing succeeded");

    private ParseResult(T value)
    {
        _isSuccess = true;
        _value = value;
        _error = null;
    }

    private ParseResult(ParseError error)
    {
        _isSuccess = false;
        _value = null;
        _error = error;
    }

    /// <summary>
    /// Creates a successful parse result.
    /// </summary>
    /// <param name="value">The parsed value.</param>
    /// <returns>A successful ParseResult.</returns>
    public static ParseResult<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new ParseResult<T>(value);
    }

    /// <summary>
    /// Creates a failed parse result.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="offset">The byte offset where parsing failed.</param>
    /// <param name="protocol">The protocol being parsed.</param>
    /// <returns>A failed ParseResult.</returns>
    public static ParseResult<T> Failure(string errorCode, string message, int offset, ProtocolType protocol = ProtocolType.Unknown)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorCode);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        return new ParseResult<T>(new ParseError(errorCode, message, offset, protocol));
    }

    /// <summary>
    /// Deconstructs the result into success/failure components.
    /// </summary>
    /// <param name="isSuccess">Whether parsing succeeded.</param>
    /// <param name="value">The parsed value (if successful).</param>
    /// <param name="error">The error (if failed).</param>
    public void Deconstruct(out bool isSuccess, out T? value, out ParseError? error)
    {
        isSuccess = _isSuccess;
        value = _value;
        error = _error;
    }

    /// <summary>
    /// Implicit conversion from successful result to the wrapped value type.
    /// </summary>
    public static implicit operator T?(ParseResult<T> result) => result.IsSuccess ? result.Value : null;

    /// <summary>
    /// Pattern matching support for success case.
    /// </summary>
    public static bool operator true(ParseResult<T> result) => result.IsSuccess;

    /// <summary>
    /// Pattern matching support for failure case.
    /// </summary>
    public static bool operator false(ParseResult<T> result) => !result.IsSuccess;
}

/// <summary>
/// Represents an error that occurred during parsing.
/// </summary>
public readonly record struct ParseError
{
    /// <summary>
    /// The error code.
    /// </summary>
    public string ErrorCode { get; init; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// The byte offset where parsing failed.
    /// </summary>
    public int Offset { get; init; }

    /// <summary>
    /// The protocol being parsed when the error occurred.
    /// </summary>
    public ProtocolType Protocol { get; init; }

    /// <summary>
    /// The raw data that was being parsed (optional).
    /// </summary>
    public string? RawData { get; init; }

    /// <summary>
    /// Creates a new ParseError.
    /// </summary>
    public ParseError(string errorCode, string message, int offset, ProtocolType protocol = ProtocolType.Unknown, string? rawData = null)
    {
        ErrorCode = errorCode;
        Message = message;
        Offset = offset;
        Protocol = protocol;
        RawData = rawData;
    }

    /// <summary>
    /// Creates a ParseError from an exception.
    /// </summary>
    public static ParseError FromException(Exception ex, int offset, ProtocolType protocol = ProtocolType.Unknown, string? rawData = null)
    {
        return new ParseError(
            ex is GpsTrackerException gpsEx ? gpsEx.GetType().Name : "PARSE_ERROR",
            ex.Message,
            offset,
            protocol,
            rawData
        );
    }

    /// <summary>
    /// Converts the error to an exception.
    /// </summary>
    public ParseException ToException()
    {
        var exception = new ParseException(Message, RawData ?? string.Empty, Protocol)
        {
            ErrorCode = ErrorCode
        };
        return exception;
    }
}
