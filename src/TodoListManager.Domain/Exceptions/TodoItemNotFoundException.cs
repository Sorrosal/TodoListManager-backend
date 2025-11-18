// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

public class TodoItemNotFoundException : DomainException
{
    public TodoItemNotFoundException(int id) : base($"TodoItem with Id {id} was not found.")
    {
    }
}
