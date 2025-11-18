// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Events;

/// <summary>
/// Domain event raised when a todo item progression is registered.
/// </summary>
public class ProgressionRegisteredEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public int TodoItemId { get; }
    public DateTime ProgressionDate { get; }
    public decimal Percent { get; }
    public decimal TotalProgress { get; }

    public ProgressionRegisteredEvent(int todoItemId, DateTime progressionDate, decimal percent, decimal totalProgress)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TodoItemId = todoItemId;
        ProgressionDate = progressionDate;
        Percent = percent;
        TotalProgress = totalProgress;
    }
}
