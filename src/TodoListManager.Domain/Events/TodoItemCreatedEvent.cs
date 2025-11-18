// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Events;

/// <summary>
/// Domain event raised when a todo item is created.
/// </summary>
public class TodoItemCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public int TodoItemId { get; }
    public string Title { get; }
    public string Category { get; }

    public TodoItemCreatedEvent(int todoItemId, string title, string category)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TodoItemId = todoItemId;
        Title = title;
        Category = category;
    }
}
