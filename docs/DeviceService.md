# DeviceService

`DeviceService` is the default implementation of `IDeviceService` (`Services/DeviceService.cs`). It manages the lifecycle of GPS tracking devices: registration, lookup by ID or IMEI, updates, deregistration, heartbeat tracking, and connection-status snapshots for fleet monitoring.

It is a thin service layer. Persistence is delegated to repositories: the unit-of-work constructor uses `IUnitOfWork.Devices` (an `IDeviceRepository`) and `IUnitOfWork.LocationData`, while the generic constructor uses a single `IRepository<Device>`.

## Responsibilities

- **Registration**: create devices from a `Device` model (validated) or, through the generic repository, from a bare device ID.
- **Lookup**: retrieve devices by ID, IMEI, status (online/offline), or all at once.
- **Heartbeat**: record that a device has been seen (last-seen time, IP address, port, connection count) and mark it online.
- **Deregistration**: soft-delete a device by setting `IsActive = false`. The record is kept.
- **Status reporting**: project devices into `DeviceStatusDto` snapshots for single devices or the whole fleet.

## Type

```csharp
namespace GpsTrackerProtocol.Services;

public class DeviceService : IDeviceService
```

The service is registered as a singleton by `AddGpsTrackerServices()` (`Configuration/DependencyInjection.cs`).

## Constructors

```csharp
public DeviceService(IUnitOfWork unitOfWork);
public DeviceService(IRepository<Device> repository);
```

| Constructor | Repositories used | Supported members |
|---|---|---|
| `DeviceService(IUnitOfWork unitOfWork)` | `unitOfWork.Devices`, `unitOfWork.LocationData` | All `IDeviceService` members |
| `DeviceService(IRepository<Device> repository)` | Generic `IRepository<Device>` only | `GetAllDevicesAsync`, `RegisterDeviceAsync(string, ...)`, `GetDeviceByIdAsync`, `UpdateDeviceStatusAsync` |

Every other member requires the unit-of-work constructor. When the service was built from the generic repository, calling one of those members (for example `GetDeviceAsync`) dereferences a null field and throws `NullReferenceException`. Use the unit-of-work constructor, or the DI container, for full functionality.

The DI container resolves the unit-of-work constructor, because `IRepository<Device>` is not registered.

## Public API

### Members declared on `IDeviceService`

```csharp
Task<Device> RegisterDeviceAsync(Device device, CancellationToken cancellationToken = default);
Task<Device?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
Task<Device?> GetDeviceByImeiAsync(string imei, CancellationToken cancellationToken = default);
Task<IEnumerable<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
Task<IEnumerable<Device>> GetOnlineDevicesAsync(CancellationToken cancellationToken = default);
Task<bool> UpdateDeviceAsync(Device device, CancellationToken cancellationToken = default);
Task<bool> DeregisterDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
Task UpdateDeviceHeartbeatAsync(string deviceId, string? ipAddress = null, int port = 0, CancellationToken cancellationToken = default);
Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId, CancellationToken cancellationToken = default);
Task<IEnumerable<DeviceStatusDto>> GetAllDeviceStatusesAsync(CancellationToken cancellationToken = default);
```

| Method | Behavior | Returns | Throws |
|---|---|---|---|
| `RegisterDeviceAsync(Device, ...)` | Validates the device with `Device.IsValid()`, rejects a duplicate IMEI, sets `LastSeen` to now and `Status` to `Offline`, then calls `CreateAsync`. | The created device. | `ArgumentNullException` if `device` is null; `ValidationException` if invalid; `DeviceException` if the IMEI is already registered. |
| `GetDeviceAsync(string, ...)` | Looks up by device ID. | The device, or `null`. | `ArgumentException` if the ID is null, empty, or whitespace. |
| `GetDeviceByImeiAsync(string, ...)` | Looks up by IMEI. | The device, or `null`. | `ArgumentException` if the IMEI is null, empty, or whitespace. |
| `GetAllDevicesAsync(...)` | Returns all devices from the repository. | A collection of devices. | — |
| `GetOnlineDevicesAsync(...)` | Returns devices with `Status == DeviceStatus.Online`. | A collection of devices. | — |
| `UpdateDeviceAsync(Device, ...)` | Confirms that a device with the same `Id` exists, validates the model, then calls `UpdateAsync`. | Always `true` on success. | `ArgumentNullException`; `DeviceException` if the device is not found; `ValidationException` if invalid. |
| `DeregisterDeviceAsync(string, ...)` | Sets `IsActive = false` and persists the change. The record is not deleted. | Always `true` on success. | `ArgumentException`; `DeviceException` if the device is not found. |
| `UpdateDeviceHeartbeatAsync(string, string?, int, ...)` | Calls `Device.UpdateHeartbeat`: sets `LastSeen` to now, `Status` to `Online`, and increments `ConnectionCount`. A non-empty `ipAddress` replaces `IpAddress`; `port` replaces `Port` only when greater than 0. | `Task` (no value). | `DeviceException` if the device is not found. |
| `GetOfflineDevicesAsync(TimeSpan, ...)` | Delegates to the repository, which returns devices whose last heartbeat is older than `timeout`. | A collection of devices. | — |
| `GetDeviceStatusAsync(string, ...)` | Returns a status snapshot for one device. | A `DeviceStatusDto`, or `null` if not found. | `ArgumentException` if the ID is null, empty, or whitespace. |
| `GetAllDeviceStatusesAsync(...)` | Returns a status snapshot for every device, materialized as a list. | A list of `DeviceStatusDto`. | — |

Notes:

- `UpdateDeviceHeartbeatAsync` uses a `port` default of `0`. Because `Device.UpdateHeartbeat` only stores ports greater than 0, the default leaves the stored port unchanged.
- `DeviceStatusDto.IsConnected` is `true` only when `Status == DeviceStatus.Online`.
- `GetAllDeviceStatusesAsync` returns a materialized `List<DeviceStatusDto>`.

### Members that exist only on the `DeviceService` class

These members are public on the class but are not part of `IDeviceService`. They are available when the concrete type is used directly. Each requires the generic-repository constructor and throws `InvalidOperationException` ("Service was not constructed with a generic repository.") when it was built from `IUnitOfWork`.

```csharp
Task<Device> RegisterDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
Task<Device?> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default);
Task UpdateDeviceStatusAsync(string deviceId, bool isActive, CancellationToken cancellationToken = default);
```

| Method | Behavior | Returns |
|---|---|---|
| `RegisterDeviceAsync(string, ...)` | Returns the existing device with this ID if there is one. Otherwise creates `new Device { Id = deviceId, IsActive = true, RegistrationDate = DateTime.UtcNow }` and adds it. | The existing or new device. |
| `GetDeviceByIdAsync(string, ...)` | Looks up by ID through the generic repository. | The device, or `null`. |
| `UpdateDeviceStatusAsync(string, bool, ...)` | Sets `IsActive` and calls `UpdateAsync`. Does nothing if the device does not exist. | `Task` (no value). |

## Behavior notes

- **ID assignment on registration.** `RegisterDeviceAsync(Device)` runs `Device.IsValid()` before it assigns an ID, and `IsValid()` requires a non-empty `Id`. A device whose `Id` is empty is rejected with `ValidationException`, so the `Guid` fallback is not reached for a default-constructed `Device`. Set `Id` before calling the method.
- **Validation rules.** `Device.IsValid()` requires a non-empty `Id` and an IMEI of 14 to 16 digits.
- **Soft deregistration.** `DeregisterDeviceAsync` keeps the record and sets `IsActive = false`. Use `GetAllDevicesAsync` and check `IsActive` if you need to filter.
- **Heartbeats and status.** `UpdateDeviceHeartbeatAsync` marks a device `Online` (through `Device.UpdateHeartbeat`). `GetOfflineDevicesAsync` only filters by heartbeat age and does not change `Status`. Registration sets `Offline`. `UpdateDeviceAsync` persists whatever `Status` the passed object carries.
- **Exceptions.** Argument-null and empty-string checks throw before any repository call. Missing devices are reported as `DeviceException` by update, deregistration, and heartbeat calls, but as `null` by lookup calls.

## Example usage

### Setting up the service

```csharp
using GpsTrackerProtocol.Configuration;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Enums;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGpsTrackerServices();

await using var provider = services.BuildServiceProvider();
var deviceService = provider.GetRequiredService<IDeviceService>();
```

### Registering a device

```csharp
var device = new Device
{
    Id = "truck-042",
    Imei = "356938035643809",
    DeviceName = "Delivery Truck 42",
    Protocol = ProtocolType.GT06
};

try
{
    var registered = await deviceService.RegisterDeviceAsync(device);
    Console.WriteLine($"Registered {registered.Id} with status {registered.Status}");
}
catch (ValidationException ex)
{
    Console.WriteLine($"Invalid device: {ex.Message}");
}
catch (DeviceException ex)
{
    Console.WriteLine($"Duplicate IMEI: {ex.Message}");
}
```

### Recording a heartbeat

```csharp
// Called from the protocol listener when a packet arrives.
await deviceService.UpdateDeviceHeartbeatAsync("truck-042", ipAddress: "203.0.113.10", port: 5023);
```

### Checking fleet health

```csharp
var online = await deviceService.GetOnlineDevicesAsync();

var stale = await deviceService.GetOfflineDevicesAsync(TimeSpan.FromMinutes(10));
foreach (var device in stale)
{
    Console.WriteLine($"{device.Id} last seen {device.LastSeen:u}");
}

var statuses = await deviceService.GetAllDeviceStatusesAsync();
foreach (var status in statuses)
{
    Console.WriteLine($"{status.DeviceId}: connected={status.IsConnected}, packets={status.ConnectionCount}");
}
```

### Deregistering a device

```csharp
var removed = await deviceService.DeregisterDeviceAsync("truck-042");
// The record remains with IsActive == false.
```

## Related

- [IDeviceService](IDeviceService.md): the interface this class implements.
- [Device](Device.md): the device model, including `IsValid()` and `UpdateHeartbeat()`.
- [DeviceServiceTests](DeviceServiceTests.md): unit tests for this service.
