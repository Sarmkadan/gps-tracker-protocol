# IGeofenceEventProcessor

The `IGeofenceEventProcessor` interface defines the contract for processing geofence events within the GPS tracker protocol. It provides methods to submit location data for geofence evaluation, manage webhook registrations for event notifications, and query the set of geofences that the tracked entity currently occupies. Implementations handle the underlying geofence logic, event dispatch, and state tracking.

## API

### `GeofenceEventProcessor`

A property that returns the concrete `GeofenceEventProcessor` instance associated with this interface. This allows callers to access implementation-specific features or configuration that are not exposed through the interface itself.

- **Type**: `GeofenceEventProcessor`
- **Parameters**: None.
- **Return value**: The underlying `GeofenceEventProcessor` object.
- **Exceptions**: None.

### `Task ProcessLocationAsync(…)`

Asynchronously processes a location update and evaluates it against the defined geofences. The method updates the internal geofence state and triggers any registered webhooks when geofence boundaries are crossed.

- **Parameters**:  
  `location` (type depends on implementation, typically a `GpsLocation` or similar data transfer object) – The location data to process.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**:  
  - `ArgumentNullException` – if the location parameter is `null`.  
  - `InvalidOperationException` – if the processor is not properly initialized.

### `void RegisterWebhook(string url)`

Registers a webhook URL that will receive HTTP callbacks when geofence events (enter, exit, dwell) occur. The same URL can be registered only once; duplicate registrations are ignored.

- **Parameters**:  
  `url` (string) – The absolute URL of the webhook endpoint.
- **Return value**: None.
- **Exceptions**:  
  - `ArgumentNullException` – if `url` is `null`.  
  - `ArgumentException` – if `url` is not a valid absolute URI.

### `void UnregisterWebhook(string url)`

Removes a previously registered webhook URL. After unregistration, no further callbacks will be sent to that endpoint. If the URL was not registered, the method does nothing.

- **Parameters**:  
  `url` (string) – The absolute URL of the webhook to remove.
- **Return value**: None.
- **Exceptions**:  
  - `ArgumentNullException` – if `url` is `null`.  
  - `ArgumentException` – if `url` is not a valid absolute URI.

### `IReadOnlySet<string> GetCurrentGeofences()`

Returns a read-only set of geofence identifiers that the tracked entity is currently inside. The set is a snapshot of the current state and will not reflect subsequent location updates unless `ProcessLocationAsync` is called again.

- **Parameters**: None.
- **Return value**: `IReadOnlySet<string>` – a set of geofence IDs (e.g., "zone-1", "restricted-area").
- **Exceptions**: None.

## Usage

### Example 1: Basic location processing and webhook registration

```csharp
public async Task ProcessGpsUpdate(IGeofenceEventProcessor processor, GpsLocation location)
{
    // Register a webhook to receive geofence events
    processor.RegisterWebhook("https://myapp.com/geofence-callback");

    // Process the location – this may trigger webhooks if geofence boundaries are crossed
    await processor.ProcessLocationAsync(location);

    // Retrieve the current set of geofences the entity is inside
    IReadOnlySet<string> activeGeofences = processor.GetCurrentGeofences();
    Console.WriteLine($"Currently inside: {string.Join(", ", activeGeofences)}");
}
```

### Example 2: Dynamic webhook management and error handling

```csharp
public async Task HandleMultipleLocations(IGeofenceEventProcessor processor)
{
    const string webhookUrl = "https://alerts.example.com/geofence";

    try
    {
        processor.RegisterWebhook(webhookUrl);

        foreach (var location in GetLocationStream())
        {
            await processor.ProcessLocationAsync(location);
        }
    }
    finally
    {
        // Clean up the webhook when no longer needed
        processor.UnregisterWebhook(webhookUrl);
    }
}
```

## Notes

- **Duplicate webhook registration**: Calling `RegisterWebhook` with a URL that is already registered has no effect and does not throw an exception.  
- **Unregistering a non‑existent webhook**: `UnregisterWebhook` is idempotent – it silently succeeds if the URL was never registered.  
- **Thread safety**:  
  - `ProcessLocationAsync` is safe to call concurrently from multiple threads; the implementation serializes location updates internally.  
  - `RegisterWebhook` and `UnregisterWebhook` are thread‑safe and can be called while `ProcessLocationAsync` is executing.  
  - `GetCurrentGeofences` returns a snapshot that is safe to read concurrently, but the snapshot may become stale immediately after acquisition.  
- **Null and invalid URLs**: Both `RegisterWebhook` and `UnregisterWebhook` throw `ArgumentNullException` for `null` URLs and `ArgumentException` for malformed URIs.  
- **State consistency**: The set returned by `GetCurrentGeofences` reflects the state after the last completed `ProcessLocationAsync` call. If no location has been processed, the set is empty.
