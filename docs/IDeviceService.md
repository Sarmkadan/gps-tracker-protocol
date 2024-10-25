# IDeviceService

The `IDeviceService` interface defines the contract for managing GPS tracking devices within the `gps-tracker-protocol` system. It provides asynchronous operations for the full lifecycle of device management, including registration, deregistration, status updates, and retrieval of device metadata and heartbeat information. This service acts as the primary abstraction for interacting with the underlying device repository and state management systems, ensuring consistent access to both individual device details and aggregate fleet status.

## API

### Constructors
*Note: The provided signature list includes `public DeviceService` entries. As `IDeviceService` is an interface, these refer to the concrete implementation class `DeviceService`.*

- **`DeviceService`**
  - Initializes a new instance of the `DeviceService` class. Specific dependency injection requirements are determined by the concrete implementation context.

### Device Registration and Retrieval

- **`Task<Device> RegisterDeviceAsync`**
  - Registers a new device within the system.
  - **Parameters:** Accepts device registration details (specific parameter types inferred from context, typically a DTO or entity).
  - **Returns:** A `Device` object representing the newly registered entity.
  - **Throws:** May throw an exception if the device ID or IMEI already exists or if validation fails.

- **`Task<Device?> GetDeviceByIdAsync`**
  - Retrieves a specific device by its unique internal identifier.
  - **Parameters:** The unique ID of the device.
  - **Returns:** A `Device` object if found; otherwise, `null`.
  - **Throws:** Typically does not throw for missing entities; returns `null` instead.

- **`Task<Device?> GetDeviceAsync`**
  - Retrieves a device based on a generic lookup criterion.
  - **Parameters:** Lookup identifier (context-dependent).
  - **Returns:** A `Device` object if found; otherwise, `null`.
  - **Throws:** May throw if the lookup criterion is invalid.

- **`Task<Device?> GetDeviceByImeiAsync`**
  - Retrieves a specific device by its International Mobile Equipment Identity (IMEI).
  - **Parameters:** The IMEI string of the device.
  - **Returns:** A `Device` object if found; otherwise, `null`.
  - **Throws:** May throw if the IMEI format is invalid.

- **`Task<IEnumerable<Device>> GetAllDevicesAsync`**
  - Retrieves a collection of all devices registered in the system.
  - **Parameters:** None.
  - **Returns:** An enumerable collection of `Device` objects.
  - **Throws:** May throw if the underlying data store is inaccessible.

- **`Task<IEnumerable<Device>> GetOnlineDevicesAsync`**
  - Retrieves a collection of devices currently marked as online.
  - **Parameters:** None.
  - **Returns:** An enumerable collection of `Device` objects with an online status.
  - **Throws:** May throw if the status filter logic encounters a data error.

- **`Task<IEnumerable<Device>> GetOfflineDevicesAsync`**
  - Retrieves a collection of devices currently marked as offline.
  - **Parameters:** None.
  - **Returns:** An enumerable collection of `Device` objects with an offline status.
  - **Throws:** May throw if the status filter logic encounters a data error.

### Device State Management

- **`Task UpdateDeviceStatusAsync`**
  - Updates the high-level status of a specific device.
  - **Parameters:** Device identifier and the new status value.
  - **Returns:** A `Task` representing the completion of the operation.
  - **Throws:** May throw if the device does not exist or the status transition is invalid.

- **`Task UpdateDeviceHeartbeatAsync`**
  - Updates the last known heartbeat timestamp for a device, indicating activity.
  - **Parameters:** Device identifier and heartbeat data/timestamp.
  - **Returns:** A `Task` representing the completion of the operation.
  - **Throws:** May throw if the device is not registered.

- **`Task<bool> UpdateDeviceAsync`**
  - Persists changes to an existing device entity.
  - **Parameters:** The modified `Device` object or update DTO.
  - **Returns:** `true` if the update was successful; `false` if no changes were applied or the device was not found.
  - **Throws:** May throw on concurrency conflicts or data validation errors.

- **`Task<bool> DeregisterDeviceAsync`**
  - Removes a device from the active registry or marks it as decommissioned.
  - **Parameters:** The unique identifier of the device to deregister.
  - **Returns:** `true` if the deregistration was successful; `false` if the device did not exist.
  - **Throws:** May throw if the device is in a state that prevents deregistration.

### Status Data Transfer Objects (DTOs)

- **`Task<DeviceStatusDto?> GetDeviceStatusAsync`**
  - Retrieves a lightweight status summary for a specific device.
  - **Parameters:** Device identifier.
  - **Returns:** A `DeviceStatusDto` if available; otherwise, `null`.
  - **Throws:** May throw if the status service is unavailable.

- **`Task<IEnumerable<DeviceStatusDto>> GetAllDeviceStatusesAsync`**
  - Retrieves status summaries for all devices in the system.
  - **Parameters:** None.
  - **Returns:** An enumerable collection of `DeviceStatusDto` objects.
  - **Throws:** May throw if the aggregation of status data fails.

## Usage

### Example 1: Registering a new device and verifying its status
This example demonstrates registering a new tracker via IMEI and immediately retrieving its initial status DTO.

```csharp
public async Task InitializeTrackerAsync(IDeviceService deviceService, string imei)
{
    // Register the new device
    var newDevice = await deviceService.RegisterDeviceAsync(new DeviceRegistration 
    { 
        Imei = imei, 
        Model = "GT06N" 
    });

    // Verify registration by fetching the status DTO
    var status = await deviceService.GetDeviceStatusAsync(newDevice.Id);

    if (status != null)
    {
        Console.WriteLine($"Device {imei} registered. Current Status: {status.State}");
    }
    else
    {
        Console.WriteLine($"Device {imei} registered, but status initialization is pending.");
    }
}
```

### Example 2: Monitoring offline devices and updating heartbeats
This example retrieves all offline devices and simulates a heartbeat update for a specific device upon receiving a packet.

```csharp
public async Task ProcessHeartbeatAsync(IDeviceService deviceService, string imei, DateTime timestamp)
{
    // Fetch the device by IMEI to get the internal ID
    var device = await deviceService.GetDeviceByImeiAsync(imei);
    
    if (device == null)
    {
        throw new InvalidOperationException($"Device with IMEI {imei} not found.");
    }

    // Update the heartbeat
    await deviceService.UpdateDeviceHeartbeatAsync(device.Id, timestamp);

    // Audit: Check how many devices are currently offline
    var offlineDevices = await deviceService.GetOfflineDevicesAsync();
    Console.WriteLine($"Heartbeat updated for {imei}. Total offline devices: {offlineDevices.Count()}");
}
```

## Notes

- **Null Handling:** Methods returning single entities (`GetDeviceByIdAsync`, `GetDeviceByImeiAsync`, `GetDeviceStatusAsync`) return `null` rather than throwing exceptions when a resource is not found. Callers must explicitly handle null returns.
- **Boolean Return Semantics:** Methods `UpdateDeviceAsync` and `DeregisterDeviceAsync` return `bool` to indicate success or failure (e.g., item not found) without throwing control-flow exceptions. Exceptions are reserved for unexpected system errors or validation failures.
- **Concurrency:** As all members are asynchronous (`Task`), the implementation is designed for non-blocking I/O. However, callers should assume that race conditions may occur during simultaneous updates to the same device entity (e.g., `UpdateDeviceAsync` vs `UpdateDeviceStatusAsync`). Optimistic concurrency control mechanisms may be employed internally; consumers should be prepared to handle potential concurrency exceptions if retry logic is required.
- **Data Consistency:** The distinction between `Device` entities and `DeviceStatusDto` suggests a separation between persistent configuration data and volatile state data. `GetOnlineDevicesAsync` and `GetOfflineDevicesAsync` rely on the freshness of the heartbeat data; there may be a slight latency between `UpdateDeviceHeartbeatAsync` completion and the reflection of that change in the offline/online lists.
