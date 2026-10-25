# FrameReassembler

`FrameReassembler` turns a raw, possibly fragmented or concatenated, TCP byte stream from a single tracker connection into complete protocol frames (GT06, H02, TK103). It is defined in `Services/FrameReassembler.cs` as a `sealed` class with no interface.

It only performs framing. It finds frame boundaries, skips garbage, and holds back incomplete tails. It does not check checksums or decode payloads; decoding is the job of the protocol parsers (`Parsers/`) and `ProtocolParserService`.

## Responsibilities

- **Reassembly:** Keeps bytes that do not yet form a complete frame and joins them with the next chunk, so a frame may arrive split across any number of reads.
- **Splitting:** Yields every complete frame in a chunk, in arrival order, so a single read can contain several frames.
- **GT06 framing:** Recognises the doubled start marker (`0x78 0x78` or `0x79 0x79`), reads the length byte, and computes the total frame length as `declaredLength + 7`. It accepts a frame only if the total length is within `GT06_MIN_FRAME_SIZE`..`GT06_MAX_FRAME_SIZE` and it ends with `0x0D 0x0A`.
- **H02 / TK103 framing:** Recognises the `$`, `*` (H02) and `(` (TK103) start bytes and ends the frame at the first CRLF (`0x0D 0x0A`). If no CRLF has arrived and the buffer exceeds `H02_MAX_FRAME_SIZE` or `TK103_MAX_FRAME_SIZE`, the leading byte is discarded and scanning resumes.
- **Resynchronisation:** Discards any byte that matches no start marker, and discards a single byte when a candidate frame has a bad length or a bad terminator. It then keeps scanning from the next byte.
- **Memory bound:** If the residual buffer is still larger than `MaxBufferSizeBytes` (8192) after extraction, the whole buffer is cleared.

## API

### Constants

- **`const int MaxBufferSizeBytes = 8192`**  
  Upper bound on the residual buffer after each `ExtractFrames` call. Exceeding it triggers `Reset()`.

### Methods

- **`IEnumerable<ReadOnlyMemory<byte>> ExtractFrames(ReadOnlySpan<byte> chunk)`**  
  Appends `chunk` to the internal buffer, then extracts every complete frame. An empty chunk is allowed and yields nothing, but it does not change buffered state.  
  *Returns:* The complete frames, in order. Each element is a fresh copy, so it stays valid after later calls. The result is empty when no full frame is available yet. The returned sequence is already materialised; it is not a lazy enumeration over the buffer.

- **`void Reset()`**  
  Discards all buffered bytes. The next call starts resynchronisation from scratch. Call it when a connection is closed or reused.

### Threading

The class is not thread-safe. Use one instance per connection and call `ExtractFrames` from one thread at a time, in the order the bytes were read.

## Example usage

A minimal read loop for one connection:

```csharp
using GpsTrackerProtocol.Services;

public static class TrackerConnectionHandler
{
    public static async Task RunAsync(Stream networkStream, CancellationToken cancellationToken)
    {
        // One reassembler per connection; never share it between sockets.
        var reassembler = new FrameReassembler();
        var buffer = new byte[1024];

        while (true)
        {
            int read = await networkStream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break; // remote side closed the connection
            }

            foreach (ReadOnlyMemory<byte> frame in reassembler.ExtractFrames(buffer.AsSpan(0, read)))
            {
                // Each frame is one complete GT06, H02 or TK103 frame.
                // Hand it to the protocol parsing layer here.
                ProcessFrame(frame);
            }
        }

        reassembler.Reset();
    }

    private static void ProcessFrame(ReadOnlyMemory<byte> frame)
    {
        // Application-specific handling.
    }
}
```

A GT06 login frame of 15 bytes (`78 78 08 … 0D 0A`) sent in two reads of 5 and 10 bytes:

```csharp
var reassembler = new FrameReassembler();

var first = reassembler.ExtractFrames(frameBytes.AsSpan(0, 5));   // empty: frame incomplete
var second = reassembler.ExtractFrames(frameBytes.AsSpan(5));     // one 15-byte frame
```

## Tests

The behaviour is covered by `tests/gps-tracker-protocol.Tests/FrameReassemblerTests.cs`:

- `ExtractFrames_CompleteFrameInOneChunk_YieldsOneFrame`: a whole frame in one chunk.
- `ExtractFrames_FrameSplitByteByByte_IsReassembled`: a frame fed one byte at a time.
- `ExtractFrames_TwoConcatenatedFrames_YieldsTwoFramesInOrder`: two frames in one chunk.
- `ExtractFrames_GarbageBeforeValidFrame_IsResynchronized`: leading garbage is skipped.
- `ExtractFrames_ExceedsMaxBufferSize_ClearsBuffer`: the buffer is cleared after overflow and the next valid frame is accepted.
- `ExtractFrames_EmptyChunk_YieldsNoFramesAndPreservesState`: an empty chunk leaves a partial frame in place.

Integration use is also exercised in `tests/gps-tracker-protocol.Tests/Services/ProtocolParserIntegrationTests.cs`, which feeds GT06 frames through the reassembler.

The tests use only GT06 frames. H02 and TK103 framing has no direct test in this file.

Run them with:

```bash
dotnet test tests/gps-tracker-protocol.Tests --filter "FullyQualifiedName~FrameReassemblerTests"
```

## Notes

- Frames are returned without checksum or payload validation. Callers must treat them as framed but unverified input.
- The overflow cap is a backstop. The per-protocol length limits and byte-by-byte resynchronisation keep the residual buffer small, so in normal operation `Reset()` from the cap is not expected to fire. The `ExtractFrames_ExceedsMaxBufferSize_ClearsBuffer` test feeds bytes that are all discarded by resynchronisation, so it does not by itself prove the cap path runs.
- `ExtractFrames` allocates a new list and a new array for every frame it returns.
