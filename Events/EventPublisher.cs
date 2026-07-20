#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Events;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Event system for decoupling location updates and journey events.
/// Allows subscribers to react to system events without tight coupling.
/// </summary>
public interface IDomainEvent
{
    string EventId { get; }
    DateTime Timestamp { get; }
    string AggregateId { get; }
}

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : IDomainEvent;
    IDisposable Subscribe<T>(Func<T, Task> handler) where T : IDomainEvent;
}

public class EventPublisher : IEventPublisher
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ILogger<EventPublisher> _logger;
    private readonly object _lock = new();

    public EventPublisher(ILogger<EventPublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event) where T : IDomainEvent
    {
        _logger.LogInformation("Publishing event: {EventType} (ID: {EventId})", typeof(T).Name, @event.EventId);

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(typeof(T), out var handlers))
            {
                _logger.LogDebug("No subscribers for event type: {EventType}", typeof(T).Name);
                return;
            }

            var tasks = handlers.Select(handler =>
            {
                try
                {
                    return ((Func<T, Task>)handler)(@event);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in event handler for {EventType}", typeof(T).Name);
                    return Task.CompletedTask;
                }
            });

            Task.WaitAll(tasks.ToArray());
        }
    }

    public IDisposable Subscribe<T>(Func<T, Task> handler) where T : IDomainEvent
    {
        lock (_lock)
        {
            var eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType))
                _subscribers[eventType] = new List<Delegate>();

            _subscribers[eventType].Add(handler);
            _logger.LogInformation("Subscriber added for event: {EventType}", eventType.Name);

            return new Unsubscriber<T>(_subscribers, eventType, handler);
        }
    }

    private class Unsubscriber<T> : IDisposable where T : IDomainEvent
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers;
        private readonly Type _eventType;
        private readonly Delegate _handler;

        public Unsubscriber(Dictionary<Type, List<Delegate>> subscribers, Type eventType, Delegate handler)
        {
            _subscribers = subscribers;
            _eventType = eventType;
            _handler = handler;
        }

        public void Dispose()
        {
            lock (_subscribers)
            {
                if (_subscribers.TryGetValue(_eventType, out var handlers))
                {
                    handlers.Remove(_handler);
                }
            }
        }
    }
}

// Event definitions
public class LocationUpdatedEvent : IDomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AggregateId { get; set; }
    public string DeviceId { get; set; }
    public LocationData Location { get; set; }
}

public class JourneyStartedEvent : IDomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AggregateId { get; set; }
    public string DeviceId { get; set; }
    public Journey Journey { get; set; }
}

public class JourneyCompletedEvent : IDomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AggregateId { get; set; }
    public string DeviceId { get; set; }
    public Journey Journey { get; set; }
    public double TotalDistance { get; set; }
    public TimeSpan Duration { get; set; }
}

public class DeviceRegisteredEvent : IDomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AggregateId { get; set; }
    public Device Device { get; set; }
}

public class CommandExecutedEvent : IDomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AggregateId { get; set; }
    public string CommandId { get; set; }
    public string DeviceId { get; set; }
    public CommandStatus Status { get; set; }
}

public enum CommandStatus
{
    Pending,
    Executing,
    Completed,
    Failed
}

// New event for speed limit alerts
public class SpeedLimitExceededEvent : IDomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AggregateId { get; set; }
    public string DeviceId { get; set; }
    public double Speed { get; set; }
    public double MaxSpeed { get; set; }
}
