// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested todo item is not found.
/// </summary>
public class TodoItemNotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TodoItemNotFoundException"/> class.
    /// </summary>
    /// <param name="id">The ID of the todo item that was not found.</param>
    public TodoItemNotFoundException(int id) : base($"TodoItem with Id {id} was not found.")
    {
    }
}
