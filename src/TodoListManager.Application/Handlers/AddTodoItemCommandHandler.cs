// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

public class AddTodoItemCommandHandler
{
    private readonly ITodoList _todoList;
    private readonly ITodoListRepository _repository;

    public AddTodoItemCommandHandler(ITodoList todoList, ITodoListRepository repository)
    {
        _todoList = todoList;
        _repository = repository;
    }

    public void Handle(AddTodoItemCommand command)
    {
        var id = _repository.GetNextId();
        _todoList.AddItem(id, command.Title, command.Description, command.Category);
    }
}
