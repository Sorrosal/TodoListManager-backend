// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;

namespace TodoListManager.Application.Handlers;

public class UpdateTodoItemCommandHandler
{
    private readonly ITodoList _todoList;

    public UpdateTodoItemCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    public void Handle(UpdateTodoItemCommand command)
    {
        _todoList.UpdateItem(command.Id, command.Description);
    }
}
