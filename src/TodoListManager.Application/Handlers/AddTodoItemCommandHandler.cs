// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to add a new todo item.
/// </summary>
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Task<Result> Handle(AddTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var id = _repository.GetNextId();
            _todoList.AddItem(id, command.Title, command.Description, command.Category);
            return Task.FromResult(Result.Success());
        }
        catch (InvalidCategoryException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
        catch (DomainException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
    }
}
