// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to add a new todo item.
/// </summary>
public class AddTodoItemCommandHandler
{
    private readonly ITodoList _todoList;
    private readonly ITodoListRepository _repository;

    public AddTodoItemCommandHandler(ITodoList todoList, ITodoListRepository repository)
    {
        _todoList = todoList;
        _repository = repository;
    }

    /// <summary>
    /// Handles the add todo item command.
    /// </summary>
    /// <param name="command">The command containing item details.</param>
    /// <returns>A result indicating success or failure.</returns>
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
