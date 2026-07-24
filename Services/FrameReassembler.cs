#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpsTrackerProtocol.Services;

using System.Buffers;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Parsers;

/// <summary>
/// Reassembles complete GT06/H02/TK103 protocol frames out of a raw, possibly
/// fragmented or concatenated, byte stream coming from a single TCP connection.
/// </summary>
/// <remarks>
/// One <see cref="FrameReassembler"/> instance must be dedicated to a single
/// connection: it keeps a small internal buffer of bytes that have arrived but
/// do not yet form a complete frame. Feed every chunk read from the socket into
/// <see cref="ExtractFrames"/>, in order, and process the frames it yields.
/// </remarks>
public sealed class FrameReassembler
{
    /// <summary>
    /// Hard cap, in bytes, on the amount of unconsumed data this reassembler
    /// will buffer while waiting for a frame to complete. Protects against a
    /// misbehaving or malicious device that never sends a valid terminator,
    /// which would otherwise grow the buffer without bound.
    /// </summary>
    public const int MaxBufferSizeBytes = 8192;

    private readonly List<byte> _buffer = new();

    /// <summary>
    /// Feeds a chunk of freshly-read socket bytes into the reassembler and
    /// returns every complete frame that can now be extracted, in arrival
    /// order. Bytes belonging to an incomplete trailing frame are retained
    /// internally and combined with data supplied on the next call.
    /// </summary>
    /// <param name="chunk">The raw bytes read from the connection.</param>
    /// <returns>
    /// The complete, validated-by-framing frames extracted from the buffered
    /// stream. Empty when no full frame is available yet.
    /// </returns>
    public IEnumerable<ReadOnlyMemory<byte>> ExtractFrames(ReadOnlySpan<byte> chunk)
    {
        if (!chunk.IsEmpty)
        {
            _buffer.AddRange(chunk.ToArray());
        }

        var frames = new List<ReadOnlyMemory<byte>>();
        while (TryExtractOneFrame(out var frame))
        {
            frames.Add(frame);
        }

        // Defense in depth: even though per-protocol resynchronization keeps
        // the buffer bounded by each protocol's own max frame size, cap the
        // residual buffer so a stream that never resembles any known start
        // marker cannot grow memory unboundedly.
        if (_buffer.Count > MaxBufferSizeBytes)
        {
            _buffer.Clear();
        }

        return frames;
    }

    /// <summary>
    /// Discards any bytes currently buffered for this connection, forcing the
    /// next call to <see cref="ExtractFrames"/> to resynchronize from scratch.
    /// </summary>
    public void Reset() => _buffer.Clear();

    /// <summary>
    /// Attempts to slice a single complete frame off the front of the internal
    /// buffer. Skips (and discards) leading garbage bytes that do not match a
    /// known start marker, and resynchronizes past frames whose declared
    /// length or terminator turns out to be corrupt.
    /// </summary>
    /// <param name="frame">The extracted frame, when the method returns true.</param>
    /// <returns>True when a complete frame was extracted; false when more data is needed.</returns>
    private bool TryExtractOneFrame(out ReadOnlyMemory<byte> frame)
    {
        frame = default;

        while (_buffer.Count > 0)
        {
            byte marker = _buffer[0];

            if (marker is ProtocolConstants.GT06_START_MARKER or ProtocolConstants.GT06_EXTENDED_START_MARKER)
            {
                if (_buffer.Count < 2)
                    return false; // need the doubled marker byte

                if (_buffer[1] != marker)
                {
                    DropBytes(1);
                    continue;
                }

                if (_buffer.Count < 3)
                    return false; // need the length byte

                int declaredLength = _buffer[2];
                int totalLength = declaredLength + 7;

                if (totalLength < ProtocolConstants.GT06_MIN_FRAME_SIZE || totalLength > ProtocolConstants.GT06_MAX_FRAME_SIZE)
                {
                    DropBytes(1);
                    continue;
                }

                if (_buffer.Count < totalLength)
                    return false; // frame not fully arrived yet

                // Validate end markers before extracting
                if (_buffer[totalLength - 2] != ProtocolConstants.GT06_END_MARKER || _buffer[totalLength - 1] != 0x0A)
                {
                    DropBytes(1);
                    continue;
                }

                frame = TakeFrame(totalLength);
                return true;
            }

            if (marker == (byte)'$' || marker == (byte)'*' || marker == ProtocolConstants.TK103_START_MARKER)
            {
                int maxFrameSize = marker == ProtocolConstants.TK103_START_MARKER
                    ? ProtocolConstants.TK103_MAX_FRAME_SIZE
                    : ProtocolConstants.H02_MAX_FRAME_SIZE;

                int terminatorIndex = FindCrLf();
                if (terminatorIndex >= 0)
                {
                    frame = TakeFrame(terminatorIndex + 2);
                    return true;
                }

                if (_buffer.Count > maxFrameSize)
                {
                    DropBytes(1);
                    continue;
                }

                return false; // terminator not seen yet, wait for more data
            }

            // Byte does not match any known protocol start marker: garbage, resync.
            DropBytes(1);
        }

        return false;
    }

    /// <summary>
    /// Scans the buffer for a CRLF (<c>0x0D 0x0A</c>) terminator, as used by
    /// the H02 and TK103 text frame formats.
    /// </summary>
    /// <returns>The index of the <c>0x0D</c> byte, or -1 if no CRLF is buffered yet.</returns>
    private int FindCrLf()
    {
        for (int i = 0; i < _buffer.Count - 1; i++)
        {
            if (_buffer[i] == 0x0D && _buffer[i + 1] == 0x0A)
                return i;
        }

        return -1;
    }

    /// <summary>Removes and returns the first <paramref name="length"/> bytes of the buffer.</summary>
    private ReadOnlyMemory<byte> TakeFrame(int length)
    {
        var data = _buffer.GetRange(0, length).ToArray();
        _buffer.RemoveRange(0, length);
        return data;
    }

    /// <summary>Discards the first <paramref name="count"/> bytes of the buffer without yielding them.</summary>
    private void DropBytes(int count) => _buffer.RemoveRange(0, count);
}
