# GpsFrame

The `GpsFrame` class serves as the primary data container for individual GPS telemetry packets within the `gps-tracker-protocol` system. It encapsulates both the raw binary payload received from tracking devices and the parsed metadata required for protocol validation, source identification, and diagnostic analysis. This type abstracts the complexities of byte-level manipulation, providing standardized access to checksum verification, header information, and formatted string representations suitable for logging or further processing.

## API

### `FrameId`
*   **Type:** `public string`
*   **Description:** A unique identifier assigned to this specific frame instance upon creation. This ID is used for tracking the packet through the processing pipeline and correlating logs.
*   **Remarks:** Guaranteed to be non-null and unique within the current application session.

### `Protocol`
*   **Type:** `public ProtocolType`
*   **Description:** Indicates the specific communication protocol version or type detected for this frame (e.g., GT06, Concox, custom binary).
*   **Remarks:** Used to determine the appropriate parser logic for the `RawData`.

### `RawData`
*   **Type:** `public byte[]`
*   **Description:** The exact sequence of bytes received from the network socket or serial port before any decoding or modification.
*   **Remarks:** This array should be treated as immutable after the frame is constructed. Modifying this array directly may invalidate the `IsValidChecksum` state.

### `ReceivedAt`
*   **Type:** `public DateTime`
*   **Description:** The precise timestamp indicating when the frame was received by the server infrastructure.
*   **Remarks:** Uses the server's local clock unless synchronized via NTP. Distinct from any timestamp embedded within the GPS payload itself.

### `SourceAddress`
*   **Type:** `public string`
*   **Description:** The network address (IP address or device identifier) from which the frame originated.
*   **Remarks:** Format depends on the transport layer (e.g., "192.168.1.50" for TCP/UDP).

### `SourcePort`
*   **Type:** `public int`
*   **Description:** The network port number associated with the incoming connection or datagram.
*   **Remarks:** Typically 0 if the transport mechanism does not utilize ports (e.g., serial connections).

### `IsValidChecksum`
*   **Type:** `public bool`
*   **Description:** A boolean flag indicating whether the checksum calculated from the `RawData` matches the expected value defined by the `Protocol`.
*   **Remarks:** A value of `false` suggests data corruption during transmission or a protocol mismatch.

### `ChecksumValue`
*   **Type:** `public string?`
*   **Description:** The hexadecimal string representation of the checksum extracted from the frame or calculated during validation.
*   **Remarks:** May be `null` if the protocol does not utilize a checksum or if the frame structure is too malformed to extract one.

### `Headers`
*   **Type:** `public Dictionary<string, string>`
*   **Description:** A collection of key-value pairs representing protocol-specific headers or metadata fields parsed from the frame.
*   **Remarks:** Contents vary significantly based on the `Protocol` type. Common keys might include "IMEI", "Command", or "Status".

### `IsValid`
*   **Type:** `public bool`
*   **Description:** A composite flag indicating the overall integrity of the frame.
*   **Remarks:** Typically returns `true` only if `IsValidChecksum` is true, `RawData` is not empty, and the basic structural requirements of the `Protocol` are met.

### `ToHex`
*   **Type:** `public string`
*   **Description:** Converts the `RawData` byte array into a formatted hexadecimal string representation.
*   **Returns:** A string containing hex values (e.g., "4A 0F B2"), usually separated by spaces.
*   **Exceptions:** None. Returns an empty string if `RawData` is null or empty.

### `ExtractBytes`
*   **Type:** `public byte[]`
*   **Description:** Extracts and returns a specific subset of bytes from the `RawData` payload, typically excluding protocol headers and footers.
*   **Returns:** A new byte array containing the core payload.
*   **Exceptions:** May throw an `InvalidOperationException` if the frame is marked as `!IsValid` or if the internal structure is unrecognized.

### `ExtractString`
*   **Type:** `public string`
*   **Description:** Decodes the extracted payload bytes into a human-readable string using the protocol's default encoding (usually ASCII or UTF-8).
*   **Returns:** The decoded string content.
*   **Exceptions:** May throw an `InvalidOperationException` if the frame is invalid. Throws `DecoderFallbackException` if the byte sequence cannot be decoded.

### `ToString`
*   **Type:** `public override string`
*   **Description:** Returns a concise string summary of the frame, including the `FrameId`, `Protocol`, `SourceAddress`, and validity status.
*   **Returns:** A formatted string suitable for logging.
*   **Remarks:** Does not include the full `RawData` to prevent log flooding.

## Usage

### Example 1: Basic Validation and Logging
This example demonstrates receiving a frame, verifying its integrity, and logging the result based on the `IsValid` and `IsValidChecksum` properties.

```csharp
public void ProcessIncomingFrame(GpsFrame frame)
{
    // Log the arrival of the frame
    Console.WriteLine($"[{frame.ReceivedAt}] Frame {frame.FrameId} received from {frame.SourceAddress}:{frame.SourcePort}");

    // Check overall validity and checksum specifically
    if (!frame.IsValid)
    {
        Console.Error.WriteLine($"Invalid frame structure detected for protocol {frame.Protocol}");
        return;
    }

    if (!frame.IsValidChecksum)
    {
        Console.Error.WriteLine($"Checksum mismatch! Expected/Found: {frame.ChecksumValue}");
        // Decide whether to discard or process with warning
        return;
    }

    // Safe to process payload
    Console.WriteLine($"Valid {frame.Protocol} frame. Hex dump: {frame.ToHex}");
}
```

### Example 2: Payload Extraction and Header Analysis
This example shows how to access specific metadata from the `Headers` dictionary and extract the core message content as a string.

```csharp
public string GetDeviceMessage(GpsFrame frame)
{
    if (!frame.IsValid)
    {
        throw new InvalidOperationException("Cannot extract message from an invalid frame.");
    }

    // Retrieve IMEI from headers if available
    string deviceId = frame.Headers.TryGetValue("IMEI", out var imei) ? imei : "Unknown";

    // Extract the core payload as a string
    try
    {
        string payload = frame.ExtractString;
        return $"Device {deviceId} reports: {payload}";
    }
    catch (Exception ex)
    {
        // Handle cases where bytes cannot be decoded to string
        return $"Device {deviceId} sent binary data that could not be decoded: {ex.Message}";
    }
}
```

## Notes

*   **Thread Safety:** The `GpsFrame` class is designed to be immutable after construction. All public properties return either value types, immutable strings, or defensive copies of collections/arrays where appropriate. However, the `Headers` dictionary is a reference type; while the reference itself is thread-safe to read, concurrent modification of the dictionary contents by multiple threads is not supported. It is recommended to treat the `Headers` collection as read-only after the frame is instantiated.
*   **Data Integrity:** The `IsValid` property is a snapshot of the frame's state at the time of validation. If the underlying `RawData` array is modified externally (which is discouraged but possible since it is a reference type), the `IsValid` and `IsValidChecksum` properties will not automatically update and may report stale information.
*   **Null Handling:** The `ChecksumValue` property is nullable (`string?`). Consumers must check for null before performing string operations on this property, as some protocols or malformed frames may not contain a calculable checksum.
*   **Encoding Assumptions:** The `ExtractString` method assumes a specific character encoding defined by the `Protocol`. If the raw bytes contain data incompatible with this encoding, the method may throw an exception or produce replacement characters, depending on the underlying decoder configuration.
