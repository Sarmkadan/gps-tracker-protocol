# ExtensionsTests

Unit tests for extension methods in the `gps-tracker-protocol` project, verifying behavior of byte array, string, and numeric conversions, checksum calculations, and validation utilities.

## API

### `ToHexString_ByteArray_ReturnsUppercaseHexWithoutDashes`
Tests that a byte array is converted to an uppercase hexadecimal string without dashes.

### `ToHexString_EmptyArray_ReturnsEmptyString`
Tests that an empty byte array returns an empty string.

### `ToHexString_WithSpaces_ReturnsDashSeparatedHex`
Tests that a byte array with space values is converted to a dash-separated hexadecimal string.

### `ToUInt16BigEndian_ValidOffset_ReturnsBigEndianValue`
Tests that a valid offset in a byte array returns the correct big-endian 16-bit unsigned integer.

### `ToUInt16BigEndian_InvalidOffset_ThrowsArgumentException`
Tests that an invalid offset throws an `ArgumentException`.

### `CalculateXorChecksum_KnownBytes_ReturnsExpectedXorResult`
Tests that a known sequence of bytes produces the expected XOR checksum result.

### `CalculateXorChecksum_SingleByte_ReturnsSameByte`
Tests that a single byte returns the same byte as its XOR checksum.

### `StartsWithMarker_MatchingPrefix_ReturnsTrue`
Tests that a byte array starting with a specific marker returns `true`.

### `StartsWithMarker_NonMatchingFirstByte_ReturnsFalse`
Tests that a byte array not starting with a specific marker returns `false`.

### `IndexOfSequence_SequencePresent_ReturnsStartIndex`
Tests that the correct starting index is returned when a sequence is present in a byte array.

### `IndexOfSequence_SequenceAbsent_ReturnsMinusOne`
Tests that `-1` is returned when a sequence is not present in a byte array.

### `CopyRange_ValidRange_ReturnsCorrectSubset`
Tests that a valid range of bytes is copied correctly into a new array.

### `ToAsciiString_ValidBytes_ReturnsDecodedString`
Tests that valid ASCII bytes are decoded into the correct string.

### `IsValidImei_FifteenDigits_ReturnsTrue`
Tests that a 15-digit string is recognized as a valid IMEI.

### `IsValidImei_TooShortString_ReturnsFalse`
Tests that a string shorter than 15 digits is not a valid IMEI.

### `IsValidImei_ContainsNonDigit_ReturnsFalse`
Tests that a string containing non-digit characters is not a valid IMEI.

### `IsValidDeviceId_AlphanumericWithDashUnderscore_ReturnsTrue`
Tests that a device ID containing alphanumeric characters, dashes, and underscores is valid.

### `IsValidDeviceId_ContainsAtSymbol_ReturnsFalse`
Tests that a device ID containing an `@` symbol is invalid.

### `SanitizeDeviceId_InvalidChars_StripsThemOut`
Tests that invalid characters are stripped from a device ID during sanitization.

### `SanitizeDeviceId_EmptyString_ReturnsUnknown`
Tests that an empty device ID is sanitized to the string `"unknown"`.

## Usage
