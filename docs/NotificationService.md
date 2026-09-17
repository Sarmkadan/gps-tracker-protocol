# NotificationService

The `NotificationService` class implements the `INotificationService` interface and provides an in-memory notification mechanism for alerting about device events such as speed violations, geofence breaches, and devices going offline. Each alert is recorded as a `Notification` object, logged through `ILogger<NotificationService>`, and stored in a thread-local list for later retrieval. The class is designed to be extended to support email, SMS, and push notification channels.

## API

The `NotificationService` class exposes the following public members:

- `SendSpeedingAlertAsync(string deviceId, double speed, double speedLimit)`: Records a speeding-violation notification for the given device. Returns a completed `Task`.
- `SendGeofenceAlertAsync(string deviceId, double latitude, double longitude)`: Records a geofence-breach notification for the given device at the specified coordinates. Returns a completed `Task`.
- `SendOfflineAlertAsync(string deviceId)`: Records a device-offline notification for the given device. Returns a completed `Task`.
- `GetNotifications(string deviceId = null)`: Retrieves all recorded notifications, or filters them by device ID when `deviceId` is provided. Returns an `IEnumerable<Notification>`.
- `MarkAsRead(string notificationId)`: Marks the notification with the given ID as read by setting its `IsRead` flag to `true`. No-op if the ID is null, empty, or not found.

The `Notification` class exposes the following public properties:

- `Id`: A unique string identifier for the notification.
- `DeviceId`: The identifier of the device the notification relates to.
- `Type`: A `NotificationType` enum value describing the kind of alert.
- `Message`: A human-readable description of the alert.
- `Timestamp`: The UTC time at which the notification was created.
- `IsRead`: A boolean indicating whether the notification has been acknowledged.

The `NotificationType` enum defines the following values: `SpeedingViolation`, `GeofenceBreach`, `DeviceOffline`, `LowBattery`, `HighTemperature`, and `CommunicationError`.

The alert methods throw an `ArgumentException` when `deviceId` is null or whitespace.

## Usage

Here are examples of using the `NotificationService` class:

```csharp
// Example 1: Sending alerts
var logger = LoggerFactory.Create(builder => builder.AddConsole())
    .CreateLogger<NotificationService>();
var service = new NotificationService(logger);

await service.SendSpeedingAlertAsync("DEV-001", 92.5, 80.0);
await service.SendGeofenceAlertAsync("DEV-001", 50.4501, 30.5234);
await service.SendOfflineAlertAsync("DEV-002");
```

```csharp
// Example 2: Querying and acknowledging notifications
var notifications = service.GetNotifications("DEV-001");
foreach (var notification in notifications)
{
    Console.WriteLine($"[{notification.Timestamp:O}] {notification.Type}: {notification.Message}");
}

var first = service.GetNotifications().FirstOrDefault();
if (first is not null)
{
    service.MarkAsRead(first.Id);
    Console.WriteLine($"Marked {first.Id} as read: {first.IsRead}");
}
```

## Notes

- Notifications are stored in memory only and are lost when the application restarts. For persistent storage, consider a database-backed implementation.
- The `_notifications` list is not synchronized; concurrent writes from multiple threads are not safe without external synchronization.
- The alert methods are synchronous in nature and return `Task.CompletedTask`; they do not perform any I/O. This is a placeholder for future email, SMS, or push delivery.
- `SendOfflineAlertAsync` logs at `LogCritical` level, while the speeding and geofence alerts log at `LogWarning` level.