# InMemoryDeviceRepository
The `InMemoryDeviceRepository` class is a data access object that provides an in-memory storage mechanism for managing devices, journeys, commands, and response messages in the context of a GPS tracking system. It allows for various query operations, data retrieval, and deletion of outdated records, all within the confines of an in-memory data store.

## API
The `InMemoryDeviceRepository` class exposes several public members that enable interaction with the in-memory data store:
- `GetByImeiAsync`: Retrieves a device by its IMEI asynchronously. Returns a `Device` object if found, otherwise `null`.
- `GetByStatusAsync`: Retrieves a collection of devices based on their status asynchronously. Returns an `IEnumerable<Device>`.
- `GetByProtocolAsync`: Retrieves a collection of devices based on the protocol they use asynchronously. Returns an `IEnumerable<Device>`.
- `GetActiveDevicesAsync`: Retrieves a collection of active devices asynchronously. Returns an `IEnumerable<Device>`.
- `GetTotalCountAsync`: Retrieves the total count of devices in the repository asynchronously. Returns an `int`.
- `GetOfflineDevicesAsync`: Retrieves a collection of offline devices asynchronously. Returns an `IEnumerable<Device>`.
- `GetByDeviceIdAsync`: Retrieves a collection of journeys associated with a specific device ID asynchronously. Returns an `IEnumerable<Journey>`.
- `GetCompletedAsync`: Retrieves a collection of completed journeys asynchronously. Returns an `IEnumerable<Journey>`.
- `GetByTimeRangeAsync`: Retrieves a collection of journeys within a specified time range asynchronously. Returns an `IEnumerable<Journey>`.
- `GetOngoingJourneyAsync`: Retrieves the ongoing journey for a device asynchronously. Returns a `Journey` object if found, otherwise `null`.
- `GetTotalDistanceAsync`: Calculates the total distance traveled by devices asynchronously. Returns a `double`.
- `DeleteOlderThanAsync`: Deletes devices older than a specified time asynchronously. Returns the number of deleted devices as an `int`.
- `GetByDeviceIdAsync` (Commands): Retrieves a collection of commands associated with a specific device ID asynchronously. Returns an `IEnumerable<Command>`.
- `GetPendingAsync`: Retrieves a collection of pending commands asynchronously. Returns an `IEnumerable<Command>`.
- `GetByStatusAsync` (Commands): Retrieves a collection of commands based on their status asynchronously. Returns an `IEnumerable<Command>`.
- `GetExpiredAsync`: Retrieves a collection of expired commands asynchronously. Returns an `IEnumerable<Command>`.
- `DeleteOlderThanAsync` (Commands): Deletes commands older than a specified time asynchronously. Returns the number of deleted commands as an `int`.
- `GetByDeviceIdAsync` (ResponseMessages): Retrieves a collection of response messages associated with a specific device ID asynchronously. Returns an `IEnumerable<ResponseMessage>`.
- `GetByCommandIdAsync`: Retrieves a collection of response messages associated with a specific command ID asynchronously. Returns an `IEnumerable<ResponseMessage>`.
- `GetByTimeRangeAsync` (ResponseMessages): Retrieves a collection of response messages within a specified time range asynchronously. Returns an `IEnumerable<ResponseMessage>`.

These methods may throw exceptions if the data store is not properly initialized, if there are issues with data retrieval, or if the specified parameters are invalid.

## Usage
Here are examples of using the `InMemoryDeviceRepository` class:
```csharp
// Example 1: Retrieving devices by status
var repository = new InMemoryDeviceRepository();
var devices = await repository.GetByStatusAsync(DeviceStatus.Active);
foreach (var device in devices)
{
    Console.WriteLine($"Device IMEI: {device.Imei}, Status: {device.Status}");
}

// Example 2: Retrieving journeys for a device
var journeys = await repository.GetByDeviceIdAsync(deviceId);
foreach (var journey in journeys)
{
    Console.WriteLine($"Journey ID: {journey.Id}, Start Time: {journey.StartTime}, End Time: {journey.EndTime}");
}
```

## Notes
- The `InMemoryDeviceRepository` class stores data in memory, which means that all data will be lost when the application restarts. For persistent storage, consider using a database-backed repository.
- This class is not designed for high-traffic or large-scale applications due to its in-memory nature. For such scenarios, a more robust data storage solution should be implemented.
- The class is thread-safe for read operations but may not be suitable for concurrent write operations without additional synchronization mechanisms.
- When using `DeleteOlderThanAsync`, ensure that the specified time is in the correct format and takes into account the system's clock to avoid unintended data loss.
- The `GetTotalDistanceAsync` method may throw an exception if there are issues calculating the total distance, such as invalid or missing location data.
