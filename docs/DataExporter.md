# DataExporter

`DataExporter` provides asynchronous methods for serializing GPS tracker protocol data into common interchange formats. It handles both individual device exports and bulk dataset exports, supporting JSON, CSV, and GeoJSON output. The class is designed for command-line or batch-processing scenarios where structured output must be written to the filesystem.

## API

### `public DataExporter`

Default constructor. Initializes a new instance of the exporter. No configuration or dependencies are required at construction time.

### `public async Task ExportToJsonAsync`

Exports the full dataset to a JSON file.

- **Parameters:** Accepts a collection of device records and a destination file path.
- **Returns:** A `Task` representing the asynchronous write operation.
- **Exceptions:** Throws `ArgumentNullException` when the data source or path is null. Throws `IOException` if the target file cannot be written (e.g., permission denied, path is a directory). Throws `System.Text.Json.JsonException` if serialization encounters an object graph that cannot be mapped.

### `public async Task ExportToCsvAsync`

Exports the full dataset to a CSV file.

- **Parameters:** Accepts a collection of device records and a destination file path.
- **Returns:** A `Task` representing the asynchronous write operation.
- **Exceptions:** Throws `ArgumentNullException` when the data source or path is null. Throws `IOException` if the target file cannot be written. The underlying writer may throw `ObjectDisposedException` if the stream is prematurely closed.

### `public async Task ExportToGeoJsonAsync`

Exports the dataset as a GeoJSON FeatureCollection, preserving spatial coordinates from the protocol data.

- **Parameters:** Accepts a collection of device records and a destination file path.
- **Returns:** A `Task` representing the asynchronous write operation.
- **Exceptions:** Throws `ArgumentNullException` when the data source or path is null. Throws `IOException` if the target file cannot be written. Throws `InvalidOperationException` if a record lacks required geometry fields.

### `public async Task ExportDevicesToJsonAsync`

Exports a filtered subset of devices to a JSON file.

- **Parameters:** Accepts a collection of device records, a predicate or device identifier list for filtering, and a destination file path.
- **Returns:** A `Task` representing the asynchronous write operation.
- **Exceptions:** Throws `ArgumentNullException` when any required argument is null. Throws `IOException` if the target file cannot be written. Throws `System.Text.Json.JsonException` on serialization failure.

### `public static async Task Main`

Entry point intended for command-line invocation. Parses arguments, instantiates `DataExporter`, loads input data, and dispatches to the appropriate export method based on the requested format flag.

- **Parameters:** Accepts a `string[]` of command-line arguments.
- **Returns:** A `Task` representing the asynchronous execution of the export pipeline.
- **Exceptions:** Throws `ArgumentException` for unrecognized or missing format flags. Propagates exceptions from the underlying export methods.

## Usage

**Example 1: Exporting all records to GeoJSON**

```csharp
var exporter = new DataExporter();
var allDevices = await DataLoader.LoadFromProtocolFileAsync("tracker-data.bin");

await exporter.ExportToGeoJsonAsync(allDevices, "output/positions.geojson");
Console.WriteLine("GeoJSON export complete.");
```

**Example 2: Command-line invocation exporting a device subset to CSV**

```csharp
// Invoked via: dotnet run -- --format csv --device-ids DEV001,DEV002 --out results.csv
public static async Task Main(string[] args)
{
    var exporter = new DataExporter();
    var allDevices = await DataLoader.LoadFromProtocolFileAsync("tracker-data.bin");

    var targetIds = args[3].Split(',');
    var filtered = allDevices.Where(d => targetIds.Contains(d.DeviceId)).ToList();

    await exporter.ExportToCsvAsync(filtered, args[5]);
}
```

## Notes

- All export methods are asynchronous and perform I/O on thread-pool threads; they are not thread-safe with respect to shared mutable state passed as the data parameter. If the source collection is modified concurrently during an export, the output is undefined.
- The `Main` method is `static` and manages its own `DataExporter` instance; it does not share state across invocations.
- GeoJSON export requires each record to contain valid longitude and latitude fields. Records missing these fields cause an `InvalidOperationException` to be thrown before any file is written.
- CSV output uses the invariant culture for numeric formatting to ensure portability across locales.
- File paths are treated as literal; the exporter does not create intermediate directories. Ensure target directories exist before calling any export method, otherwise an `IOException` (specifically `DirectoryNotFoundException`) will propagate.
