#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Interface for GPS tracker protocol parsers
// =====================================================================

namespace GpsTrackerProtocol.Parsers;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Interface for parsing GPS tracker protocol frames.
/// </summary>
/// <remarks>
/// Implementations handle one <see cref="ProtocolType"/> each. Parsing reports failures through
/// <see cref="ParseResult{T}"/> rather than throwing, so callers should check the result instead
/// of relying on exceptions.
/// </remarks>
public interface IProtocolParser
{
    /// <summary>
    /// Gets the protocol type this parser handles.
    /// </summary>
    ProtocolType ProtocolType { get; }

    /// <summary>
    /// Parses a frame from raw byte data.
    /// </summary>
    /// <param name="frameData">The raw frame data to parse.</param>
    /// <returns>
    /// A <see cref="ParseResult{T}"/> containing either the parsed <see cref="LocationData"/>
    /// or a parse error.
    /// </returns>
    ParseResult<LocationData> Parse(ReadOnlySpan<byte> frameData);

    /// <summary>
    /// Parses a frame that has already been wrapped in a <see cref="GpsFrame"/>.
    /// </summary>
    /// <param name="frame">The GPS frame to parse.</param>
    /// <returns>
    /// A <see cref="ParseResult{T}"/> containing either the parsed <see cref="LocationData"/>
    /// or a parse error.
    /// </returns>
    ParseResult<LocationData> Parse(GpsFrame frame);

    /// <summary>
    /// Validates whether the given frame data appears to be valid for this protocol.
    /// </summary>
    /// <remarks>
    /// This is a lightweight structural check (for example, frame size, protocol markers and
    /// checksum where the protocol defines one). It does not
    /// guarantee that <see cref="Parse(ReadOnlySpan{byte})"/> will succeed.
    /// </remarks>
    /// <param name="frameData">The frame data to validate.</param>
    /// <returns>True if the frame appears valid for this protocol, false otherwise.</returns>
    bool Validate(ReadOnlySpan<byte> frameData);

    /// <summary>
    /// Gets a human-readable name for this parser, intended for logging and diagnostics.
    /// </summary>
    string ParserName { get; }
}
