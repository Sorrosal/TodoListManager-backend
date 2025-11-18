// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to remove a todo item.
/// </summary>
public class RemoveTodoItemCommandHandler : IRequestHandler<RemoveTodoItemCommand, Result>
{
    private readonly ITodoList _todoList;

    public RemoveTodoItemCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    /// <summary>
    /// Handles the remove todo item command.
    /// </summary>
    /// <param name="command">The command containing the item ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Task<Result> Handle(RemoveTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _todoList.RemoveItem(command.Id);
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
