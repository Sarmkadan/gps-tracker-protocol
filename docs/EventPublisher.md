# EventPublisher

The `EventPublisher` class implements the `IEventPublisher` interface and provides a thread-safe, in-memory event publishing mechanism for the GPS tracker protocol system. It enables decoupled communication between components through the publish-subscribe pattern, allowing domain events to be broadcast to interested subscribers without creating direct dependencies.

## API

### Constructor

*   **`EventPublisher(ILogger<EventPublisher> logger)`**
    Creates a new instance of the `EventPublisher` class.
    *   `logger`: An `ILogger<EventPublisher>` instance used for logging publish/subscribe activities and errors.

### Methods

*   **`async Task PublishAsync<T>(T @event)` where T : IDomainEvent**
    Asynchronously publishes a domain event to all registered subscribers of type T.
    *   `@event`: The domain event instance to publish, which must implement the `IDomainEvent` interface.
    *   Returns: A `Task` representing the asynchronous publish operation.
    *   Behavior:
        *   Logs the publication attempt at Information level.
        *   Thread-safely retrieves all subscribers for the event type T.
        *   If no subscribers exist, logs a Debug message and returns immediately.
        *   Executes all subscriber handlers concurrently, wrapping each in error handling to prevent one handler's failure from affecting others.
        *   Waits for all handlers to complete before returning.
        *   Logs any handler exceptions at Error level but continues processing other handlers.

*   **`IDisposable Subscribe<T>(Func<T, Task> handler)` where T : IDomainEvent**
    Registers a handler to receive events of type T.
    *   `handler`: An asynchronous function that processes events of type T.
    *   Returns: An `IDisposable` token that, when disposed, unsubscribes the handler from receiving further events.
    *   Behavior:
        *   Thread-safely adds the handler to the subscriber list for type T.
        *   Creates a new subscriber list for type T if one doesn't exist.
        *   Logs the subscription addition at Information level.

### Supporting Types

*   **`Unsubscriber<T>`**
    A private nested class that implements `IDisposable` to manage event subscription lifecycle.
    *   Returned by the `Subscribe<T>` method to allow consumers to unsubscribe from events.
    *   When disposed, removes the associated handler from the subscriber list for its event type.

*   **Domain Events**
    The `EventPublisher.cs` file includes several predefined domain event types that implement `IDomainEvent`:
    *   `LocationUpdatedEvent`: Represents a device location update.
    *   `JourneyStartedEvent`: Signals the beginning of a device journey.
    *   `JourneyCompletedEvent`: Signals the end of a device journey with distance and duration metrics.
    *   `DeviceRegisteredEvent`: Indicates a new device has been registered in the system.
    *   `CommandExecutedEvent`: Represents the execution status of a device command.
    *   `SpeedLimitExceededEvent`: Alerts when a device exceeds a configured speed limit.

## Usage

### Publishing Events

```csharp
// Example: Publishing a location update event
public async Task UpdateDeviceLocation(string deviceId, LocationData location)
{
    var locationEvent = new LocationUpdatedEvent
    {
        DeviceId = deviceId,
        Location = location,
        AggregateId = deviceId // Typically the device ID serves as the aggregate ID
    };
    
    await _eventPublisher.PublishAsync(locationEvent);
}
```

### Subscribing to Events

```csharp
// Example: Event handler subscription using disposable pattern
public class LocationUpdateHandler : IDisposable
{
    private readonly IDisposable _subscription;
    private readonly ILogger<LocationUpdateHandler> _logger;
    
    public LocationUpdateHandler(EventPublisher publisher, ILogger<LocationUpdateHandler> logger)
    {
        _logger = logger;
        _subscription = publisher.Subscribe<LocationUpdatedEvent>(HandleLocationUpdated);
    }
    
    private async Task HandleLocationUpdated(LocationUpdatedEvent evt)
    {
        _logger.LogInformation("Device {DeviceId} location updated at {Timestamp}", 
                              evt.DeviceId, evt.Timestamp);
        // Process the location update (e.g., store in database, check geofences, etc.)
    }
    
    public void Dispose() => _subscription.Dispose();
}

// Example: Direct subscription with inline handler
using var subscription = _eventPublisher.Subscribe<JourneyCompletedEvent>(async evt =>
{
    Console.WriteLine($"Journey completed for device {evt.DeviceId}: " +
                     $"{evt.TotalDistance}km in {evt.Duration}");
    // Trigger notifications, update dashboards, etc.
});

// Event handler executes when publisher.PublishAsync(new JourneyCompletedEvent { ... }) is called
```

### Event Definition Best Practices

When creating new domain events:
```csharp
public class NewEventType : IDomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AggregateId { get; set; } // Typically the entity ID this event pertains to
    
    // Event-specific properties
    public string DeviceId { get; set; }
    // Add other relevant properties...
}
```

## Notes

*   **Thread Safety:** The `EventPublisher` implementation is fully thread-safe for concurrent publication and subscription operations. Internal locking ensures that subscriber lists are not modified while event handlers are executing.

*   **Error Handling:** If an event handler throws an exception, it is caught and logged at Error level, but other handlers for the same event continue to execute. This prevents a single faulty handler from disrupting event delivery to other subscribers.

*   **Subscription Lifecycle:** Consumers must properly dispose of the `IDisposable` returned by `Subscribe<T>` to avoid memory leaks. Failure to unsubscribe can result in handlers continuing to receive events after the subscriber is no longer active.

*   **Handler Performance:** Event handlers should avoid performing long-running or blocking operations directly. For asynchronous work that may take significant time, consider offloading to background queues or workers to maintain publisher responsiveness.

*   **Event Ordering:** While handlers for a single event execution are started concurrently, there is no guaranteed ordering of completion or execution across different event publications.

*   **Memory Considerations:** Events and their handlers remain in memory for as long as subscriptions exist. Long-lived subscribers should be disposed of when no longer needed to prevent accumulation of stale references.