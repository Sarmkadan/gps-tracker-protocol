# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## IDomainEvent

The `IDomainEvent` interface represents a domain event in the system. It provides metadata such as the event ID, timestamp, and aggregate ID, which can be used to track and manage events in the system.

Example usage:

```csharp
using GpsTrackerProtocol.Events;

public class EventPublisherExample
{
    public async Task PublishEventAsync()
    {
        // Create a new event
        var locationUpdatedEvent = new LocationUpdatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            AggregateId = "device123",
            DeviceId = "device123",
            Location = new LocationData
            {
                Latitude = 37.7749,
                Longitude = -122.4194
            }
        };

        // Publish the event
        var eventPublisher = new EventPublisher();
        await eventPublisher.PublishAsync(locationUpdatedEvent);
    }
}
```

<!-- (rest of README.md remains the same) -->

## GeofenceEnteredEvent

The `GeofenceEnteredEvent` represents a device crossing into a geofence zone. It contains metadata such as the event ID, timestamp, device and geofence identifiers, and the device's location and speed at the moment of entry.

Example usage:

```csharp
using System;
using GpsTrackerProtocol.Events;

var geofenceEvent = new GeofenceEnteredEvent
{
    EventId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    AggregateId = "device123",
    DeviceId = "device123",
    GeofenceId = "zoneA",
    Latitude = 37.7749,
    Longitude = -122.4194,
    Speed = 45.0
};

Console.WriteLine($"Event {geofenceEvent.EventId} for device {geofenceEvent.DeviceId} entered geofence {geofenceEvent.GeofenceId} at {geofenceEvent.Timestamp}.");
```
