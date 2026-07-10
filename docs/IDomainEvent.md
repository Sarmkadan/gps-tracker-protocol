# IDomainEvent

`IDomainEvent` serves as the foundational interface within the `gps-tracker-protocol` project for representing domain-specific occurrences. It ensures that all domain events adhere to a consistent structure, facilitating reliable communication between system components and supporting decoupled, event-driven architectures.

## API

### Properties

*   **`string EventId`**
    A unique identifier for the specific event instance.

*   **`DateTime Timestamp`**
    The date and time in UTC when the event occurred.

*   **`string AggregateId`**
    The identifier of the domain aggregate root associated with the event.

*   **`string DeviceId`**
    The identifier of the tracking device that generated the event data.

*   **`LocationData Location`**
    Provides spatial context for the event, if applicable.

*   **`Journey Journey`**
    Provides contextual data regarding the trip or journey, if applicable.

### Methods and Associated Components

*   **`EventPublisher`**
    The service class responsible for managing event distribution and maintaining the registry of active subscribers.

*   **`async Task PublishAsync<T>(T domainEvent)`**
    Asynchronously broadcasts a domain event of type `T` to all registered subscribers. Implementations must ensure that event propagation is handled robustly to prevent blocking the caller.

*   **`IDisposable Subscribe<T>(Action<T> handler)`**
    Registers a subscriber for a specific event type `T`. Returns an `IDisposable` object that must be invoked to cease receiving event notifications.

*   **`Unsubscriber`**
    A helper type used to manage the lifecycle of an event subscription. It encapsulates the logic required to remove a handler from the `EventPublisher`.

*   **`void Dispose()`**
    Releases resources associated with the subscription or publisher. When called on an `IDisposable` returned by `Subscribe`, it ensures the handler is removed and prevents potential memory leaks.

## Usage

### Publishing a Domain Event

```csharp
public async Task HandleCommand(Command cmd, EventPublisher publisher)
{
    var domainEvent = new DeviceMovedEvent 
    { 
        EventId = Guid.NewGuid().ToString(),
        Timestamp = DateTime.UtcNow,
        DeviceId = cmd.DeviceId,
        Location = cmd.NewLocation
    };
    
    await publisher.PublishAsync(domainEvent);
}
```

### Subscribing to Domain Events

```csharp
public class EventListener : IDisposable
{
    private readonly IDisposable _subscription;

    public EventListener(EventPublisher publisher)
    {
        _subscription = publisher.Subscribe<DeviceMovedEvent>(HandleDeviceMoved);
    }

    private void HandleDeviceMoved(DeviceMovedEvent evt)
    {
        Console.WriteLine($"Device {evt.DeviceId} moved at {evt.Timestamp}");
    }

    public void Dispose() => _subscription.Dispose();
}
```

## Notes

*   **Thread Safety:** The `EventPublisher` implementation must be thread-safe to support concurrent publication and subscription management. Subscribers are responsible for ensuring that their `Action<T>` handlers are thread-safe if the publisher invokes them concurrently or from different threads.
*   **Subscription Lifecycle:** It is critical to manage the `IDisposable` returned by `Subscribe`. Failure to dispose of this object can lead to memory leaks and unexpected behavior where handlers continue to be invoked after the interested component is no longer active.
*   **Event Handling:** Event handlers should remain performant and should not perform blocking I/O operations directly within the `Action<T>` delegate to avoid degrading the performance of the publisher. If long-running tasks are required, they should be offloaded to a background task or queue.
