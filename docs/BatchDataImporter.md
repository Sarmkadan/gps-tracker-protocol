# BatchDataImporter
The `BatchDataImporter` type is designed to facilitate the importation of large datasets into the gps-tracker-protocol system. It provides a set of methods for importing data from various sources, including CSV and JSON files, as well as devices. This allows for efficient and flexible data ingestion, enabling the system to process and analyze large volumes of data.

## API
* `public BatchDataImporter`: The constructor for the `BatchDataImporter` class, used to create a new instance.
* `public async Task ImportCsvAsync`: Imports data from a CSV file. This method is asynchronous, allowing it to run without blocking the calling thread. It does not take any parameters, but its implementation may throw exceptions if the import process fails.
* `public async Task ImportJsonAsync`: Imports data from a JSON file. Like `ImportCsvAsync`, this method is asynchronous and does not take any parameters. It may throw exceptions if the import process fails.
* `public async Task ImportDevicesAsync`: Imports data from devices. This method is also asynchronous and does not take any parameters. It may throw exceptions if the import process fails.
* `public static async Task Main`: The main entry point for the `BatchDataImporter` class. This method is asynchronous and does not take any parameters. It may throw exceptions if the execution fails.

## Usage
The following examples demonstrate how to use the `BatchDataImporter` class:
```csharp
// Example 1: Importing CSV data
var importer = new BatchDataImporter();
await importer.ImportCsvAsync();

// Example 2: Importing JSON data and handling exceptions
try
{
    var importer = new BatchDataImporter();
    await importer.ImportJsonAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Import failed: {ex.Message}");
}
```

## Notes
When using the `BatchDataImporter` class, consider the following edge cases and thread-safety remarks:
* The `ImportCsvAsync`, `ImportJsonAsync`, and `ImportDevicesAsync` methods are asynchronous, which means they can be used to import data without blocking the calling thread. However, if these methods are called concurrently, the import processes may interfere with each other.
* The `Main` method is also asynchronous, allowing it to be used as the entry point for an asynchronous program.
* The `BatchDataImporter` class does not provide any built-in thread-safety mechanisms, so it is up to the caller to ensure that instances are accessed and used in a thread-safe manner.
* If an exception occurs during the import process, it will be propagated to the caller, allowing them to handle the error as needed.
