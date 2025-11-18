// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Events;

/// <summary>
/// Base interface for domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Unique identifier for this event.
    /// </summary>
    public Guid EventId { get; }
}
