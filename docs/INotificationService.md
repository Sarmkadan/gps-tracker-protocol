# INotificationService

A service interface for sending and managing device-related notifications such as speeding alerts, geofence boundary crossings, and offline status warnings. It provides methods to dispatch alerts asynchronously and retrieve or update notification state.

## API

### `public Task SendSpeedingAlertAsync(...)`

Sends an asynchronous alert indicating that a device has exceeded a configured speed threshold.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw if the underlying communication channel fails or if the device is unreachable.

### `public Task SendGeofenceAlertAsync(...)`

Sends an asynchronous alert indicating that a device has entered or exited a predefined geofence boundary.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw if the underlying communication channel fails or if the device is unreachable.

### `public Task SendOfflineAlertAsync(...)`

Sends an asynchronous alert indicating that a device has gone offline or failed to report within an expected interval.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw if the underlying communication channel fails or if the device is unreachable.

### `public IEnumerable<Notification> GetNotifications()`

Retrieves a sequence of notifications associated with this service instance.

- **Parameters**: None.
- **Return value**: An `IEnumerable<Notification>` containing zero or more notification objects.
- **Exceptions**: Does not throw under normal operation.

### `public void MarkAsRead(...)`

Marks a specific notification as read by updating its `IsRead` flag.

- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: May throw if the notification does not exist or if the operation fails due to persistence issues.

### `public string Id`

Gets the unique identifier of the notification service instance.

- **Return value**: A `string` representing the service instance ID.
- **Exceptions**: Never throws.

### `public string DeviceId`

Gets the identifier of the device associated with this notification service.

- **Return value**: A `string` representing the device ID.
- **Exceptions**: Never throws.

### `public NotificationType Type`

Gets the type of notification handled by this service instance (e.g., speeding, geofence, offline).

- **Return value**: A `NotificationType` enum value.
- **Exceptions**: Never throws.

### `public string Message`

Gets the human-readable message associated with the notification.

- **Return value**: A `string` containing the alert message.
- **Exceptions**: Never throws.

### `public DateTime Timestamp`

Gets the date and time when the notification was generated.

- **Return value**: A `DateTime` indicating when the event occurred.
- **Exceptions**: Never throws.

### `public bool IsRead`

Gets or sets whether the notification has been acknowledged by the user.

- **Return value**: A `bool` indicating read status (`true` if read).
- **Exceptions**: Setting may throw if persistence fails.

## Usage
