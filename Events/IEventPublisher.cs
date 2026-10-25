#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Events;

using System;

public interface IDomainEvent
{
    string EventId { get; }
    DateTime Timestamp { get; }
    string AggregateId { get; }
}

/// <summary>
/// Publishes domain events to in-process subscribers, keyed by event type.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to all handlers subscribed to its type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The domain event type.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <returns>A task that completes once the event has been dispatched to the subscribed handlers.</returns>
    Task PublishAsync<T>(T @event) where T : IDomainEvent;

    /// <summary>
    /// Registers a handler that is invoked for every published event of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The domain event type to subscribe to.</typeparam>
    /// <param name="handler">The asynchronous callback to invoke for each matching event.</param>
    /// <returns>An <see cref="IDisposable"/> that removes the handler when disposed.</returns>
    IDisposable Subscribe<T>(Func<T, Task> handler) where T : IDomainEvent;
}
