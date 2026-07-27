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

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : IDomainEvent;
    IDisposable Subscribe<T>(Func<T, Task> handler) where T : IDomainEvent;
}
