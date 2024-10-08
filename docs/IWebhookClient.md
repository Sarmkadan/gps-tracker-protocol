# IWebhookClient

The `IWebhookClient` interface defines a contract for sending real-time event notifications to a configured webhook endpoint. It is used to transmit location updates, journey completions, device status changes, and geofence events from a GPS tracking system to an external service via HTTP POST requests.

## API

### `WebhookClient`
The concrete implementation of `IWebhookClient` responsible for executing webhook requests.

### `Task SendLocationUpdateAsync`
**Purpose**
Sends a location update event to the configured `WebhookUrl`. The payload includes the device's current coordinates, timestamp, and associated metadata.

**Parameters**
None.

**Return Value**
A `Task` representing the asynchronous operation.

**Exceptions**
- `HttpRequestException`: Thrown if the webhook request fails (e.g., network issues, invalid URL, or non-2xx response).
- `InvalidOperationException`: Thrown if the client is not active (`IsActive` is `false`).

---

### `Task SendJourneyCompletedAsync`
**Purpose**
Sends a journey completion event to the configured `WebhookUrl`. The payload includes the journey summary, such as start/end times, distance traveled, and device identifier.

**Parameters**
None.

**Return Value**
A `Task` representing the asynchronous operation.

**Exceptions**
- `HttpRequestException`: Thrown if the webhook request fails.
- `InvalidOperationException`: Thrown if the client is not active.

---

### `Task SendDeviceStatusAsync`
**Purpose**
Sends a device status update to the configured `WebhookUrl`. The payload includes the device's current operational state (e.g., battery level, signal strength, or error conditions).

**Parameters**
None.

**Return Value**
A `Task` representing the asynchronous operation.

**Exceptions**
- `HttpRequestException`: Thrown if the webhook request fails.
- `InvalidOperationException`: Thrown if the client is not active.

---

### `Task SendGeofenceEventAsync`
**Purpose**
Sends a geofence event notification to the configured `WebhookUrl`. The payload includes the geofence identifier, event type (entry/exit), and the device's location at the time of the event.

**Parameters**
None.

**Return Value**
A `Task` representing the asynchronous operation.

**Exceptions**
- `HttpRequestException`: Thrown if the webhook request fails.
- `InvalidOperationException`: Thrown if the client is not active.

---

### `string EventType`
**Purpose**
Gets the type of the event being transmitted (e.g., `"location_update"`, `"journey_completed"`, `"device_status"`, `"geofence_event"`).

**Returns**
A `string` representing the event type.

---

### `DateTime Timestamp`
**Purpose**
Gets the timestamp of the event in UTC.

**Returns**
A `DateTime` representing the event's occurrence time.

---

### `object Data`
**Purpose**
Gets the payload data associated with the event. The structure of this object depends on the event type (e.g., location coordinates for `SendLocationUpdateAsync`, geofence details for `SendGeofenceEventAsync`).

**Returns**
An `object` containing the event-specific data.

---

### `string Id`
**Purpose**
Gets the unique identifier for this webhook client instance.

**Returns**
A `string` representing the client's ID.

---

### `string DeviceId`
**Purpose**
Gets the identifier of the device associated with this webhook client.

**Returns**
A `string` representing the device's ID.

---

### `string WebhookUrl`
**Purpose**
Gets the URL to which webhook events are sent.

**Returns**
A `string` representing the target webhook endpoint.

---

### `bool IsActive`
**Purpose**
Indicates whether the webhook client is active and permitted to send events. If `false`, calls to send methods will throw an `InvalidOperationException`.

**Returns**
A `bool` where `true` means the client is active.

---

### `DateTime CreatedAt`
**Purpose**
Gets the UTC timestamp when the webhook client was instantiated.

**Returns**
A `DateTime` representing the creation time.

## Usage

### Example 1: Sending a Location Update
