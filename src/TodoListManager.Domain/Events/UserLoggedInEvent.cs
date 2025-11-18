// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Events;

/// <summary>
/// Domain event raised when a user successfully logs in.
/// </summary>
public class UserLoggedInEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public int UserId { get; }
    public string Username { get; }

    public UserLoggedInEvent(int userId, string username)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        Username = username;
    }
}
