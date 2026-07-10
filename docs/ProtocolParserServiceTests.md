# ProtocolParserServiceTests

Unit test suite for the `ProtocolParserService` class, verifying protocol detection, frame validation, and frame parsing logic for supported GPS tracker protocols (GT06, H02, TK103). Tests cover marker-based protocol detection, checksum validation, coordinate parsing (including hemispheric handling), and edge cases for empty or malformed input.

## API

### `ProtocolParserServiceTests`

Public test class containing test cases for protocol parsing functionality. No direct members are exposed beyond test method invocations.

### `DetectProtocolAsync_ShouldReturnGT06_WhenRawDataStartsWithGT06Marker`

Verifies that the protocol detection logic correctly identifies GT06 protocol when raw data begins with the GT06 marker (`##GT06##`).

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `DetectProtocolAsync_ShouldReturnTK103_WhenRawDataStartsWithTK103Marker`

Verifies that the protocol detection logic correctly identifies TK103 protocol when raw data begins with the TK103 marker (`##TK103##`).

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `DetectProtocolAsync_ShouldReturnH02_WhenRawDataStartsWithH02Marker`

Verifies that the protocol detection logic correctly identifies H02 protocol when raw data begins with the H02 marker (`##H02##`).

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `DetectProtocolAsync_ShouldReturnUnknown_WhenRawDataDoesNotMatchAnyKnownProtocol`

Verifies that the protocol detection logic returns an unknown protocol identifier when raw data does not match any supported protocol marker.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `DetectProtocolAsync_ShouldThrowArgumentException_WhenRawDataIsEmpty`

Verifies that the protocol detection logic throws an `ArgumentException` when provided with empty raw data input.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: `ArgumentException` when raw data is empty

### `ValidateFrameAsync_GT06_ShouldReturnTrue_ForValidFrame`

Verifies that the GT06 frame validation logic returns `true` for a syntactically valid GT06 frame.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ValidateFrameAsync_GT06_ShouldReturnFalse_ForInvalidChecksum`

Verifies that the GT06 frame validation logic returns `false` when the frame contains an invalid checksum.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ValidateFrameAsync_GT06_ShouldReturnFalse_ForTooShortFrame`

Verifies that the GT06 frame validation logic returns `false` when the frame is too short to be valid.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ValidateFrameAsync_H02_ShouldReturnTrue_Always`

Verifies that the H02 frame validation logic always returns `true`, regardless of input content.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ValidateFrameAsync_TK103_ShouldReturnTrue_Always`

Verifies that the TK103 frame validation logic always returns `true`, regardless of input content.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ParseFrameAsync_GT06_ShouldParseCorrectly`

Verifies that the GT06 frame parsing logic correctly extracts and converts fields from a valid GT06 frame.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ParseFrameAsync_GT06_SouthWestCoordinates_ShouldParseCorrectly`

Verifies that the GT06 frame parsing logic correctly handles coordinates in the southwestern hemisphere (negative latitude and longitude).

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ParseFrameAsync_H02_HqFormat_ShouldParseCorrectly`

Verifies that the H02 frame parsing logic correctly extracts fields from an HQ-formatted H02 frame.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ParseFrameAsync_H02_HqFormat_EasternHemisphere_ShouldProducePositiveLongitude`

Verifies that the H02 frame parsing logic produces a positive longitude value when coordinates are in the eastern hemisphere.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

### `ParseFrameAsync_TK103_ShouldParseCorrectly`

Verifies that the TK103 frame parsing logic correctly extracts and converts fields from a valid TK103 frame.

- **Parameters**: None (test method)
- **Return value**: `Task` (async test assertion)
- **Throws**: Not applicable (test framework handles assertions)

## Usage
