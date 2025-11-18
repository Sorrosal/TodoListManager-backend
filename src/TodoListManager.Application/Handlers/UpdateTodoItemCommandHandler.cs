// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to update an existing todo item.
/// </summary>
public class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand, Result>
{
    private readonly ITodoList _todoList;

    public UpdateTodoItemCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    /// <summary>
    /// Handles the update todo item command.
    /// </summary>
    /// <param name="command">The command containing the item ID and new description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Task<Result> Handle(UpdateTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _todoList.UpdateItem(command.Id, command.Description);
            return Task.FromResult(Result.Success());
        }
        catch (TodoItemNotFoundException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
        catch (TodoItemCannotBeModifiedException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
        catch (DomainException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
    }
}
