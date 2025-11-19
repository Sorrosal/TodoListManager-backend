// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to modify or remove a todo item that has more than 50% progress.
/// </summary>
public class TodoItemCannotBeModifiedException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TodoItemCannotBeModifiedException"/> class.
    /// </summary>
    /// <param name="id">The ID of the todo item that cannot be modified.</param>
    public TodoItemCannotBeModifiedException(int id) 
        : base($"TodoItem with Id {id} cannot be modified or removed because it has more than 50% progress.")
    {
    }
}
