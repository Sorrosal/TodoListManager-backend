// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
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

    public Result Handle(AddTodoItemCommand command)
    {
        try
        {
            var id = _repository.GetNextId();
            _todoList.AddItem(id, command.Title, command.Description, command.Category);
            return Result.Success();
        }
        catch (InvalidCategoryException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
