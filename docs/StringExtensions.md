# StringExtensions

A collection of pure‑function helpers for parsing, validating, and manipulating strings that are commonly used when processing GPS tracker protocols (NMEA sentences, device identifiers, hex colours, etc.). All members are static and have no external dependencies, making them safe to call from any thread.

## API

### ToDoubleOrDefault
```csharp
public static double ToDoubleOrDefault(string s, double defaultValue = 0.0)
```
* **Purpose** – Attempts to parse `s` as a double; returns `defaultValue` when parsing fails.
* **Parameters**  
  * `s` – The string to parse.  
  * `defaultValue` – Value to return if `s` cannot be parsed (default 0.0).
* **Return** – The parsed double or `defaultValue`.
* **Exceptions** – Throws `ArgumentNullException` if `s` is `null`.

### ToIntOrDefault
```csharp
public static int ToIntOrDefault(string s, int defaultValue = 0)
```
* **Purpose** – Attempts to parse `s` as an integer; returns `defaultValue` when parsing fails.
* **Parameters**  
  * `s` – The string to parse.  
  * `defaultValue` – Value to return if `s` cannot be parsed (default 0).
* **Return** – The parsed integer or `defaultValue`.
* **Exceptions** – Throws `ArgumentNullException` if `s` is `null`.

### SplitNmea
```csharp
public static string[] SplitNmea(string nmeaSentence)
```
* **Purpose** – Splits an NMEA sentence into its comma‑separated fields, preserving empty fields.
* **Parameters**  
  * `nmeaSentence` – The full NMEA sentence (including the leading `$` and optional checksum).
* **Return** – An array of strings representing each field.
* **Exceptions** – Throws `ArgumentNullException` if `nmeaSentence` is `null`.

### GetNmeaChecksum
```csharp
public static string GetNmeaChecksum(string nmeaSentence)
```
* **Purpose** – Computes the XOR checksum for an NMEA sentence (the part between `$` and `*`).
* **Parameters**  
  * `nmeaSentence` – The NMEA sentence **without** the leading `$` and trailing `*XX`.
* **Return** – A two‑character uppercase hexadecimal string (e.g., `"7A"`).
* **Exceptions** –  
  * `ArgumentNullException` if `nmeaSentence` is `null`.  
  * `ArgumentException` if the string contains a `*` character (checksum already present).

### RemoveNmeaChecksum
```csharp
public static string RemoveNmeaChecksum(string nmeaSentence)
```
* **Purpose** – Strips the optional checksum (`*XX`) from an NMEA sentence.
* **Parameters**  
  * `nmeaSentence` – The NMEA sentence possibly containing a checksum.
* **Return** – The sentence with the checksum and preceding `*` removed; if no checksum exists, the original string is returned unchanged.
* **Exceptions** – Throws `ArgumentNullException` if `nmeaSentence` is `null`.

### IsValidImei
```csharp
public static bool IsValidImei(string imei)
```
* **Purpose** – Checks whether `imei` is a valid 15‑digit IMEI that passes the Luhn algorithm.
* **Parameters**  
  * `imei` – The IMEI string to validate.
* **Return** – `true` if the IMEI is valid; otherwise `false`.
* **Exceptions** – Throws `ArgumentNullException` if `imei` is `null`.

### IsValidDeviceId
```csharp
public static bool IsValidDeviceId(string deviceId)
```
* **Purpose** – Validates a device identifier according to the project’s rules (alphanumeric, length 1‑20, no spaces).
* **Parameters**  
  * `deviceId` – The device identifier to validate.
* **Return** – `true` if the identifier conforms to the rules; otherwise `false`.
* **Exceptions** – Throws `ArgumentNullException` if `deviceId` is `null`.

### Truncate
```csharp
public static string Truncate(string input, int maxLength)
```
* **Purpose** – Returns `input` shortened to `maxLength` characters; if truncation occurs, an ellipsis (`…`) is appended.
* **Parameters**  
  * `input` – The string to truncate.  
  * `maxLength` – Maximum length of the returned string **including** the ellipsis. Must be ≥ 0.
* **Return** – The original string if its length ≤ `maxLength`; otherwise the truncated string with ellipsis.
* **Exceptions** –  
  * `ArgumentNullException` if `input` is `null`.  
  * `ArgumentOutOfRangeException` if `maxLength` is negative.

### HexToByteArray
```csharp
public static byte[] HexToByteArray(string hex)
```
* **Purpose** – Converts a hexadecimal string (e.g., `"0A3F"`) into a byte array.
* **Parameters**  
  * `hex` – The hex string to convert. May contain uppercase or lowercase letters; leading `0x` is not permitted.
* **Return** – A byte array where each pair of hex characters becomes one byte.
* **Exceptions** –  
  * `ArgumentNullException` if `hex` is `null`.  
  * `ArgumentException` if `hex` length is odd or contains non‑hex characters.

### SanitizeDeviceId
```csharp
public static string SanitizeDeviceId(string deviceId)
```
* **Purpose** – Produces a safe device identifier by removing any characters that are not letters or digits and trimming whitespace.
* **Parameters**  
  * `deviceId` – The raw identifier to sanitize.
* **Return** – A new string containing only alphanumeric characters from the original, in the same order. Returns an empty string if no valid characters remain.
* **Exceptions** – Throws `ArgumentNullException` if `deviceId` is `null`.

### IsValidHexColor
```csharp
public static bool IsValidHexColor(string color)
```
* **Purpose** – Determines whether `color` represents a valid HTML hex colour (`#RRGGBB` or `RRGGBB`).
* **Parameters**  
  * `color` – The colour string to test.
* **Return** – `true` if the string matches the pattern; otherwise `false`.
* **Exceptions** – Throws `ArgumentNullException` if `color` is `null`.

## Usage

```csharp
using GpsTrackerProtocol.Extensions; // assuming the namespace

// Example 1: Parsing an NMEA GPGGA sentence and validating its checksum
string nmea = "$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47";
if (StringExtensions.GetNmeaChecksum(nmea.TrimStart('$')) == "47")
{
    var fields = StringExtensions.SplitNmea(nmea);
    double latitude = StringExtensions.ToDoubleOrDefault(fields[2]);
    // further processing...
}

// Example 2: Validating and sanitising a device identifier before storage
string rawId = "  Device-ID_123! ";
if (StringExtensions.IsValidDeviceId(rawId))
{
    string clean = StringExtensions.SanitizeDeviceId(rawId);
    // clean == "DeviceID123"
    SaveDeviceId(clean);
}
else
{
    // fallback to a sanitized version even if the original was invalid
    string clean = StringExtensions.SanitizeDeviceId(rawId);
    if (!string.IsNullOrEmpty(clean))
        SaveDeviceId(clean);
}
```

## Notes

* All extension methods are **pure** – they depend only on their input parameters and have no side effects. Consequently, they are thread‑safe and can be invoked concurrently from multiple threads without additional synchronization.
* Methods that parse numeric values (`ToDoubleOrDefault`, `ToIntOrDefault`) never throw for format errors; they return the supplied default value. Callers should still guard against `null` inputs.
* `SplitNmea` preserves empty fields to match the behaviour of NMEA parsers that rely on positional fields (e.g., an empty altitude field).
* The checksum methods assume the standard NMEA XOR‑based checksum; they do not support proprietary extensions.
* `Truncate` counts the ellipsis character as part of `maxLength`. If `maxLength` is less than the length of the ellipsis (`…`), the method will return just the ellipsis (or throw if `maxLength` is negative, as validated).
* `HexToByteArray` expects an even‑length hex string; odd lengths are considered invalid and will raise an `ArgumentException`.
* Validation methods (`IsValidImei`, `IsValidDeviceId`, `IsValidHexColor`) return `false` for malformed input rather than throwing, except for the explicit `null` check. This allows callers to treat validation as a simple boolean test.
