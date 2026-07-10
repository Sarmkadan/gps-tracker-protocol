# IProtocolHandler

Defines the contract for protocol-specific handlers in the GPS tracker system. Implementations are responsible for detecting protocol types, validating incoming data, and constructing `GpsFrame` objects from raw protocol payloads.

## API

### `ProtocolAutoDetector`

A delegate type used to detect protocol compatibility from raw data. Implementations should return `true` if the provided data matches the protocol they handle.

- **Parameters**:
  - `data`: The raw byte array to inspect.
- **Returns**: `bool` indicating whether the data is compatible with the protocol.
- **Throws**: No exceptions documented.

### `ProtocolType`

A property exposing the protocol identifier handled by the current instance. Used by the system to route data to the correct handler.

- **Type**: `ProtocolType` (assumed enum or struct from the project).
- **Returns**: The protocol identifier.
- **Throws**: No exceptions documented.

### `Detect(ReadOnlySpan<byte> data)`

Determines if the provided data matches the protocol this handler is designed to process.

- **Parameters**:
  - `data`: The raw byte array to inspect.
- **Returns**: `bool` indicating whether the data is compatible with the protocol.
- **Throws**: No exceptions documented.

### `GetHandler(ProtocolType type)`

Retrieves a protocol handler instance for the specified protocol type.

- **Parameters**:
  - `type`: The protocol identifier to resolve.
- **Returns**: An `IProtocolHandler` instance if available; otherwise, `null`.
- **Throws**: No exceptions documented.

### `CanHandle(ProtocolType type)`

Checks whether the handler can process data for the given protocol type.

- **Parameters**:
  - `type`: The protocol identifier to check.
- **Returns**: `bool` indicating compatibility.
- **Throws**: No exceptions documented.

### `CreateFrameAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)`

Constructs a `GpsFrame` from the provided protocol payload.

- **Parameters**:
  - `payload`: The raw protocol data to parse.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Returns**: A `Task<GpsFrame>` representing the asynchronous operation.
- **Throws**:
  - `ArgumentNullException`: If `payload` is null.
  - `FormatException`: If the payload is malformed for the protocol.
  - `InvalidOperationException`: If the handler cannot process the payload due to an internal state issue.

## Usage

### Example 1: Detecting and Handling a Protocol
