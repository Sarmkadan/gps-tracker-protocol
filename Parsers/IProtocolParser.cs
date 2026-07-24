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
    /// <returns>A ParseResult containing either the parsed LocationData or a ParseError.</returns>
    ParseResult<LocationData> Parse(ReadOnlySpan<byte> frameData);

    /// <summary>
    /// Parses a frame from a GpsFrame object.
    /// </summary>
    /// <param name="frame">The GPS frame to parse.</param>
    /// <returns>A ParseResult containing either the parsed LocationData or a ParseError.</returns>
    ParseResult<LocationData> Parse(GpsFrame frame);

    /// <summary>
    /// Validates whether the given frame data appears to be valid for this protocol.
    /// </summary>
    /// <param name="frameData">The frame data to validate.</param>
    /// <returns>True if the frame appears valid, false otherwise.</returns>
    bool Validate(ReadOnlySpan<byte> frameData);

    /// <summary>
    /// Gets a human-readable name for this parser.
    /// </summary>
    string ParserName { get; }
}
