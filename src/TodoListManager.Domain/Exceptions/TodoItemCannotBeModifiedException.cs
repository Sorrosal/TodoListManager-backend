// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

public class TodoItemCannotBeModifiedException : DomainException
{
    public TodoItemCannotBeModifiedException(int id) 
        : base($"TodoItem with Id {id} cannot be modified or removed because it has more than 50% progress.")
    {
    }
}
