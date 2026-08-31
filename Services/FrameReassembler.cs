#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpsTrackerProtocol.Services;

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

    private byte[] _buffer = Array.Empty<byte>();
    private int _head;
    private int _count;

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
            EnsureCapacity(chunk.Length);
            chunk.CopyTo(_buffer.AsSpan(_head + _count));
            _count += chunk.Length;
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
        if (_count > MaxBufferSizeBytes)
        {
            Reset();
        }

        return frames;
    }

    /// <summary>
    /// Discards any bytes currently buffered for this connection, forcing the
    /// next call to <see cref="ExtractFrames"/> to resynchronize from scratch.
    /// </summary>
    public void Reset()
    {
        _head = 0;
        _count = 0;
    }

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

        while (_count > 0)
        {
            byte marker = _buffer[_head];

            if (marker is ProtocolConstants.GT06_START_MARKER or ProtocolConstants.GT06_EXTENDED_START_MARKER)
            {
                if (_count < 2)
                    return false; // need the doubled marker byte

                if (_buffer[_head + 1] != marker)
                {
                    DropBytes(1);
                    continue;
                }

                if (_count < 3)
                    return false; // need the length byte

                int declaredLength = _buffer[_head + 2];
                int totalLength = declaredLength + 7;

                if (totalLength < ProtocolConstants.GT06_MIN_FRAME_SIZE || totalLength > ProtocolConstants.GT06_MAX_FRAME_SIZE)
                {
                    DropBytes(1);
                    continue;
                }

                if (_count < totalLength)
                    return false; // frame not fully arrived yet

                // Validate end markers before extracting
                if (_buffer[_head + totalLength - 2] != ProtocolConstants.GT06_END_MARKER || _buffer[_head + totalLength - 1] != 0x0A)
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

                if (_count > maxFrameSize)
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
        for (int i = 0; i < _count - 1; i++)
        {
            if (_buffer[_head + i] == 0x0D && _buffer[_head + i + 1] == 0x0A)
                return i;
        }

        return -1;
    }

    /// <summary>Removes and returns the first <paramref name="length"/> bytes of the buffer.</summary>
    private ReadOnlyMemory<byte> TakeFrame(int length)
    {
        var data = _buffer.AsSpan(_head, length).ToArray();
        DropBytes(length);
        return data;
    }

    /// <summary>Discards the first <paramref name="count"/> bytes of the buffer without yielding them.</summary>
    private void DropBytes(int count)
    {
        _head += count;
        _count -= count;

        if (_count == 0)
        {
            _head = 0;
        }
    }

    /// <summary>Ensures the buffer has contiguous space for newly arrived bytes.</summary>
    private void EnsureCapacity(int additionalCount)
    {
        int requiredCount = checked(_count + additionalCount);
        if (requiredCount <= _buffer.Length - _head)
        {
            return;
        }

        if (requiredCount <= _buffer.Length)
        {
            _buffer.AsSpan(_head, _count).CopyTo(_buffer);
            _head = 0;
            return;
        }

        int newCapacity = Math.Max(requiredCount, Math.Max(256, _buffer.Length * 2));
        var newBuffer = new byte[newCapacity];
        _buffer.AsSpan(_head, _count).CopyTo(newBuffer);
        _buffer = newBuffer;
        _head = 0;
    }
}
