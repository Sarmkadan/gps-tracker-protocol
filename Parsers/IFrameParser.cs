#nullable enable

namespace GpsTrackerProtocol.Parsers;

using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// A standardized interface for parsing GPS tracker protocol frames.
/// </summary>
/// <typeparam name="T">The type of successfully parsed data model.</typeparam>
public interface IFrameParser<T> where T : class
{
    /// <summary>
    /// Parses a frame from raw byte data.
    /// </summary>
    /// <param name="frameData">The raw frame data to parse.</param>
    /// <returns>A ParseResult containing either the parsed data or a ParseError.</returns>
    /// <exception cref="ArgumentNullException">Thrown if input is null (if applicable, though ReadOnlySpan is a struct).</exception>
    ParseResult<T> Parse(ReadOnlySpan<byte> frameData);

    /// <summary>
    /// Attempts to parse a frame from raw byte data without throwing exceptions for malformed frames.
    /// </summary>
    /// <param name="frameData">The raw frame data to parse.</param>
    /// <param name="result">The parsed result.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    bool TryParse(ReadOnlySpan<byte> frameData, out T? result);

    /// <summary>
    /// Validates whether the given frame data appears to be valid for this protocol.
    /// </summary>
    /// <param name="frameData">The frame data to validate.</param>
    /// <returns>True if the frame appears valid, false otherwise.</returns>
    bool Validate(ReadOnlySpan<byte> frameData);
}
