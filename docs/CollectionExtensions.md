# CollectionExtensions

The `CollectionExtensions` class provides a set of static extension methods and utility functions designed to simplify common collection operations within the `gps-tracker-protocol` project. It offers specialized functionality for data segmentation, statistical calculation, order-preserving deduplication, and safe element retrieval, enabling concise and readable data processing pipelines for telemetry and tracking data.

## API

### `Chunk<T>`
Splits a sequence into chunks of a specified size.
*   **Purpose**: Partitions an `IEnumerable<T>` into smaller enumerables, where each chunk contains a maximum number of elements defined by the size parameter.
*   **Parameters**:
    *   `source`: The source sequence to chunk.
    *   `size`: The maximum size of each chunk.
*   **Return Value**: An `IEnumerable<IEnumerable<T>>` representing the sequence of chunks.
*   **Throws**: Throws an exception if `size` is less than or equal to zero, or if `source` is null.

### `Median<T>`
Calculates the median value of a numeric sequence.
*   **Purpose**: Computes the statistical median of a collection of numbers.
*   **Parameters**:
    *   `source`: The sequence of numeric values.
*   **Return Value**: A `double` representing the median value.
*   **Throws**: Throws an exception if `source` is null or empty. May throw if elements cannot be cast to or operated on as numeric types.

### `DistinctByOrder<T, TKey>`
Returns distinct elements from a sequence based on a specified key selector while preserving the original order of first occurrence.
*   **Purpose**: Filters a sequence to remove duplicates based on a derived key, ensuring the first instance of any key is retained in its original position.
*   **Parameters**:
    *   `source`: The source sequence.
    *   `keySelector`: A function to extract the key from each element.
*   **Return Value**: An `IEnumerable<T>` containing the distinct elements.
*   **Throws**: Throws an exception if `source` or `keySelector` is null.

### `MinMax<T>`
Determines the minimum and maximum values in a sequence in a single pass.
*   **Purpose**: Efficiently retrieves both the smallest and largest elements from a collection.
*   **Parameters**:
    *   `source`: The source sequence of comparable items.
*   **Return Value**: A tuple `(T min, T max)` containing the minimum and maximum values.
*   **Throws**: Throws an exception if `source` is null or empty.

### `PercentageWhere<T>`
Calculates the percentage of elements in a sequence that satisfy a given condition.
*   **Purpose**: Computes the ratio of matching elements to the total count, expressed as a percentage.
*   **Parameters**:
    *   `source`: The source sequence.
    *   `predicate`: The condition to test each element against.
*   **Return Value**: A `double` representing the percentage (0.0 to 100.0).
*   **Throws**: Throws an exception if `source` or `predicate` is null. Returns 0.0 if the source is empty (does not throw on empty collection).

### `SafeGet<T>`
Retrieves an element at a specific index with bounds checking.
*   **Purpose**: Safely accesses an element by index without throwing an `IndexOutOfRangeException`.
*   **Parameters**:
    *   `source`: The source list or array.
    *   `index`: The zero-based index of the element to get.
    *   `defaultValue`: The value to return if the index is out of range.
*   **Return Value**: The element at the specified index, or `defaultValue` if the index is invalid.
*   **Throws**: Throws an exception if `source` is null.

### `SlidingWindow<T>`
Generates a sequence of sliding windows over the source collection.
*   **Purpose**: Creates subsets of the sequence where each subsequent window shifts by one element.
*   **Parameters**:
    *   `source`: The source sequence.
    *   `size`: The size of the sliding window.
*   **Return Value**: An `IEnumerable<IEnumerable<T>>` representing the sliding windows.
*   **Throws**: Throws an exception if `size` is less than or equal to zero, if `size` is greater than the source count, or if `source` is null.

## Usage

**Example 1: Processing Telemetry Batches and Calculating Statistics**
This example demonstrates chunking a large stream of GPS points for batch processing and calculating the median speed for a specific trip segment.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// Assume gpsPoints is a large collection of telemetry data
List<GpsPoint> gpsPoints = GetTelemetryStream();

// Process points in batches of 50
var batches = gpsPoints.Chunk(50);
foreach (var batch in batches)
{
    ProcessBatch(batch.ToList());
}

// Calculate the median speed from a specific subset
List<double> speeds = gpsPoints.Select(p => p.Speed).ToList();
double medianSpeed = speeds.Median();

Console.WriteLine($"Median Speed: {medianSpeed} km/h");
```

**Example 2: Deduplication and Safe Access**
This example shows how to remove duplicate sensor readings based on a timestamp key while maintaining order, and safely retrieving a specific record.

```csharp
using System;
using System.Collections.Generic;

// Sensor readings with potential duplicates
var readings = new List<SensorReading>
{
    new SensorReading { Id = 1, Timestamp = DateTime.UtcNow },
    new SensorReading { Id = 2, Timestamp = DateTime.UtcNow }, // Duplicate time
    new SensorReading { Id = 3, Timestamp = DateTime.UtcNow.AddSeconds(1) }
};

// Keep only the first reading for any given timestamp
var uniqueReadings = readings.DistinctByOrder(r => r.Timestamp).ToList();

// Safely get the 5th element without risking an exception
SensorReading fifthReading = uniqueReadings.SafeGet(4, defaultValue: null);

if (fifthReading != null)
{
    LogReading(fifthReading);
}
else
{
    Console.WriteLine("Index out of range; default value returned.");
}
```

## Notes

*   **Deferred Execution**: Methods returning `IEnumerable<T>` (such as `Chunk`, `DistinctByOrder`, and `SlidingWindow`) utilize deferred execution. The source collection is not iterated until the resulting sequence is enumerated. Modifying the source collection during enumeration may result in undefined behavior or exceptions.
*   **Empty Collections**: `Median<T>` and `MinMax<T>` will throw an exception if the source collection is empty, as these statistical operations require at least one element. `PercentageWhere<T>` handles empty collections gracefully by returning `0.0`.
*   **Thread Safety**: This class is stateless and thread-safe for concurrent read operations provided the underlying source collections passed to the methods are not modified concurrently. If the source collection is being modified by another thread, external synchronization is required.
*   **Memory Usage**: `SlidingWindow` and `Chunk` create new enumerable structures. For very large datasets, be mindful that materializing these results (e.g., via `.ToList()`) will increase memory consumption proportional to the dataset size.
