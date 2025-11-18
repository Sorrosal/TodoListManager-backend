// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;

namespace TodoListManager.Application.Handlers;

public class RegisterProgressionCommandHandler
{
    private readonly ITodoList _todoList;

    public RegisterProgressionCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    public void Handle(RegisterProgressionCommand command)
    {
        _todoList.RegisterProgression(command.Id, command.Date, command.Percent);
    }
}
