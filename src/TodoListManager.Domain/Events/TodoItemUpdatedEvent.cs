// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Events;

/// <summary>
/// Domain event raised when a todo item is updated.
/// </summary>
public class TodoItemUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public int TodoItemId { get; }
    public string NewDescription { get; }

    public TodoItemUpdatedEvent(int todoItemId, string newDescription)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TodoItemId = todoItemId;
        NewDescription = newDescription;
    }
}
