# GeofenceEnteredEvent

The `GeofenceEnteredEvent` records the specific occurrence of a device crossing into the boundary of a defined geographic area. This event is fundamental to systems tracking asset location, alerting on boundary violations, or triggering proximity-based workflows within the `gps-tracker-protocol`. It provides necessary telemetry data, including the device's location and speed at the precise time of entry, to enable downstream services to process and react to the event efficiently.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `EventId` | `string` | A globally unique identifier for this specific event instance. |
| `Timestamp` | `DateTime` | The date and time, in UTC, when the device entered the geofence. |
| `AggregateId` | `string` | The identifier for the aggregate (e.g., trip or vehicle) associated with this event. |
| `DeviceId` | `string` | The unique identifier of the GPS tracking device. |
| `GeofenceId` | `string` | The unique identifier of the geofence that was entered. |
| `Latitude` | `double` | The latitude coordinate of the device at the moment of entry. |
| `Longitude` | `double` | The longitude coordinate of the device at the moment of entry. |
| `Speed` | `double` | The speed of the device at the moment of entry, measured in kilometers per hour (km/h). |
| `DwellDuration`| `TimeSpan` | The duration spent within the context leading to this event; defaults to `TimeSpan.Zero` if not applicable to the immediate entry. |

## Usage

### Instantiating and Publishing an Event
```csharp
var geofenceEvent = new GeofenceEnteredEvent
{
    EventId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    AggregateId = "trip-8832",
    DeviceId = "dev-123",
    GeofenceId = "zone-north",
    Latitude = 45.523062,
    Longitude = -122.676482,
    Speed = 45.5,
    DwellDuration = TimeSpan.Zero
};

await _eventBus.PublishAsync(geofenceEvent);
```

### Consuming the Event in a Subscriber
```csharp
public void Handle(GeofenceEnteredEvent @event)
{
    Console.WriteLine($"Device {@event.DeviceId} entered {@event.GeofenceId} at {@event.Timestamp}.");
    
    if (@event.Speed > 100.0)
    {
        _alertService.TriggerHighSpeedEntryAlert(@event.DeviceId, @event.GeofenceId);
    }
}
```

## Notes

*   **Thread Safety**: The `GeofenceEnteredEvent` object is designed to be treated as immutable once published. If modifications are required, a new instance or a deep copy must be created to ensure thread safety across multiple event handlers and prevent race conditions.
*   **Data Integrity**: Before publishing, ensure that `Latitude` and `Longitude` values adhere to valid geographic constraints (latitude within -90 to 90; longitude within -180 to 180).
*   **Serialization**: When using automated serializers (e.g., JSON), ensure that `DateTime` fields are serialized in UTC format to maintain consistency across distributed systems.
