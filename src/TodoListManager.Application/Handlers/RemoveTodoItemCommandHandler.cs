// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;

namespace TodoListManager.Application.Handlers;

public class RemoveTodoItemCommandHandler
{
    private readonly ITodoList _todoList;

    public RemoveTodoItemCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    public void Handle(RemoveTodoItemCommand command)
    {
        _todoList.RemoveItem(command.Id);
    }
}
