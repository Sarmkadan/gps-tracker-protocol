# ByteExtensions

Utility class providing extension and conversion methods for byte arrays and individual bytes, commonly used in GPS tracker protocol parsing and checksum validation.

## API

### `ToHexString`

Converts a byte array to its hexadecimal string representation using uppercase characters without separators.

- **Parameters**
  - `bytes` (byte[]): The byte array to convert. Must not be null.
- **Returns**
  - `string`: A hexadecimal string representation of the input bytes.
- **Throws**
  - `ArgumentNullException`: If `bytes` is null.

---

### `ToUInt16BigEndian`

Converts a two-byte sequence from big-endian byte order to a `ushort`.

- **Parameters**
  - `bytes` (byte[]): The byte array containing at least two bytes starting at index 0. Must not be null and must have length ≥ 2.
  - `startIndex` (int): The starting index in the array. Must be non-negative and such that `startIndex + 2 ≤ bytes.Length`.
- **Returns**
  - `ushort`: The 16-bit unsigned integer represented by the two bytes in big-endian order.
- **Throws**
  - `ArgumentNullException`: If `bytes` is null.
  - `ArgumentOutOfRangeException`: If `startIndex` is negative or if there are fewer than two bytes available from `startIndex`.

---

### `ToUInt32BigEndian`

Converts a four-byte sequence from big-endian byte order to a `uint`.

- **Parameters**
  - `bytes` (byte[]): The byte array containing at least four bytes starting at index 0. Must not be null and must have length ≥ 4.
  - `startIndex` (int): The starting index in the array. Must be non-negative and such that `startIndex + 4 ≤ bytes.Length`.
- **Returns**
  - `uint`: The 32-bit unsigned integer represented by the four bytes in big-endian order.
- **Throws**
  - `ArgumentNullException`: If `bytes` is null.
  - `ArgumentOutOfRangeException`: If `startIndex` is negative or if there are fewer than four bytes available from `startIndex`.

---
### `CalculateXorChecksum`

Computes the XOR checksum over a byte array.

- **Parameters**
  - `bytes` (byte[]): The byte array to checksum. Must not be null.
- **Returns**
  - `byte`: The XOR checksum value.
- **Throws**
  - `ArgumentNullException`: If `bytes` is null.

---
### `ToAsciiString`

Converts a byte array to a string using ASCII encoding, replacing invalid bytes with a placeholder character.

- **Parameters**
  - `bytes` (byte[]): The byte array to convert. Must not be null.
- **Returns**
  - `string`: A string representation of the ASCII characters in the input bytes.
- **Throws**
  - `ArgumentNullException`: If `bytes` is null.

---
### `StartsWithMarker`

Determines whether a byte array begins with a specified marker byte sequence.

- **Parameters**
  - `bytes` (byte[]): The byte array to check. Must not be null.
  - `marker` (byte[]): The marker byte sequence to match. Must not be null.
- **Returns**
  - `bool`: `true` if `bytes` starts with `marker`; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If either `bytes` or `marker` is null.

---
### `IndexOfSequence`

Finds the first occurrence of a byte sequence within a byte array.

- **Parameters**
  - `bytes` (byte[]): The byte array to search. Must not be null.
  - `sequence` (byte[]): The byte sequence to locate. Must not be null.
- **Returns**
  - `int`: The zero-based index of the first occurrence of `sequence` in `bytes`, or `-1` if not found.
- **Throws**
  - `ArgumentNullException`: If either `bytes` or `sequence` is null.

---
### `CopyRange`

Creates a new byte array containing a subset of the original array.

- **Parameters**
  - `bytes` (byte[]): The source byte array. Must not be null.
  - `offset` (int): The zero-based starting index of the range to copy. Must be non-negative and such that `offset < bytes.Length`.
  - `count` (int): The number of bytes to copy. Must be non-negative and such that `offset + count ≤ bytes.Length`.
- **Returns**
  - `byte[]`: A new byte array containing the copied bytes.
- **Throws**
  - `ArgumentNullException`: If `bytes` is null.
  - `ArgumentOutOfRangeException`: If `offset` or `count` is negative, or if the range exceeds the bounds of `bytes`.

## Usage
