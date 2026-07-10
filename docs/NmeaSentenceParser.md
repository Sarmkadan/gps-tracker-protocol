# NmeaSentenceParser

Parses raw NMEA 0183 sentences from GPS receivers into structured `LocationData` objects. Handles both single-sentence parsing and batch processing of multi-line buffers, with optional checksum validation and coordinate conversion utilities.

## API

### `public bool ValidateChecksum`

Gets or sets whether the parser validates the NMEA checksum (the `*XX` suffix) on each sentence before processing. When `true`, sentences with missing or incorrect checksums are silently skipped. Default is `false`.

### `public LocationData ParseSentence(string sentence)`

Parses a single NMEA sentence string and returns a populated `LocationData` instance.

**Parameters:**
- `sentence` — A raw NMEA sentence (e.g. `"$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47"`). Leading `$` and trailing checksum are expected but handled based on `ValidateChecksum`.

**Returns:**
A `LocationData` object with fields extracted from the sentence. Fields not present in the sentence type remain at their default values.

**Throws:**
- `ArgumentNullException` — if `sentence` is `null`.
- `FormatException` — if the sentence is malformed beyond recovery (e.g. unparseable numeric fields in critical positions).

### `public IReadOnlyList<LocationData> ParseBuffer(string buffer)`

Parses a buffer containing zero or more newline-separated NMEA sentences and returns a list of `LocationData` objects for all successfully parsed sentences.

**Parameters:**
- `buffer` — A string potentially containing multiple NMEA sentences delimited by `\r\n`, `\n`, or `\r`.

**Returns:**
An `IReadOnlyList<LocationData>` containing one entry per valid sentence. Sentences that fail checksum validation (when enabled) or are otherwise unparseable are omitted from the result. Returns an empty list if no valid sentences are found.

**Throws:**
- `ArgumentNullException` — if `buffer` is `null`.

### `public static double ConvertNmeaCoordinate(string coordinate, string hemisphere)`

Converts a raw NMEA coordinate string and its hemisphere indicator to a signed decimal degree value.

**Parameters:**
- `coordinate` — A coordinate in NMEA format (e.g. `"4807.038"` for 48°07.038', or `"01131.000"` for 11°31.000').
- `hemisphere` — A single character indicating direction: `"N"`, `"S"`, `"E"`, or `"W"`.

**Returns:**
A `double` representing the coordinate in decimal degrees. Positive for North and East, negative for South and West.

**Throws:**
- `ArgumentNullException` — if either parameter is `null`.
- `FormatException` — if `coordinate` is not a valid numeric string in expected NMEA format, or if `hemisphere` is not one of the four recognized characters.

## Usage

### Example 1: Parsing a single sentence with checksum validation

```csharp
var parser = new NmeaSentenceParser
{
    ValidateChecksum = true
};

string sentence = "$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47";

try
{
    LocationData location = parser.ParseSentence(sentence);
    Console.WriteLine($"Lat: {location.Latitude}, Lon: {location.Longitude}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Failed to parse sentence: {ex.Message}");
}
```

### Example 2: Batch processing a multi-line buffer

```csharp
var parser = new NmeaSentenceParser();

string buffer = @"
$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47
$GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
$GPGLL,4807.038,N,01131.000,E,123519,A*31
";

IReadOnlyList<LocationData> locations = parser.ParseBuffer(buffer);

foreach (var loc in locations)
{
    Console.WriteLine($"Lat: {loc.Latitude:F6}, Lon: {loc.Longitude:F6}");
}
// Output: 3 entries (one per valid sentence)
```

## Notes

- **Checksum behavior:** When `ValidateChecksum` is `false` (default), the parser ignores the `*XX` suffix entirely. Sentences without a checksum are processed normally. When `true`, any sentence with a missing or mismatched checksum is discarded without throwing an exception.
- **Buffer parsing resilience:** `ParseBuffer` treats each line independently. A malformed sentence on one line does not affect parsing of subsequent lines. Empty lines and lines not starting with `$` are ignored.
- **Coordinate conversion:** `ConvertNmeaCoordinate` is a pure static utility method. It does not depend on parser state and can be used independently for raw coordinate manipulation.
- **Thread safety:** `NmeaSentenceParser` instance members are not thread-safe. Changing `ValidateChecksum` while another thread calls `ParseSentence` or `ParseBuffer` may produce inconsistent results. The static `ConvertNmeaCoordinate` method is thread-safe and can be called concurrently without synchronization.
- **Sentence types:** The parser recognizes standard NMEA talker IDs and sentence formatters (`GGA`, `RMC`, `GLL`, `GSA`, `GSV`, `VTG`, `ZDA`). Unrecognized sentence types are silently skipped in `ParseBuffer` and return a minimally populated `LocationData` from `ParseSentence`.
