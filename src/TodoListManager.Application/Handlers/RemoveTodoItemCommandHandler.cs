// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to remove a todo item.
/// </summary>
public class RemoveTodoItemCommandHandler
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
    /// <returns>A result indicating success or failure.</returns>
    public Result Handle(RemoveTodoItemCommand command)
    {
        try
        {
            _todoList.RemoveItem(command.Id);
            return Result.Success();
        }
        catch (TodoItemNotFoundException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (TodoItemCannotBeModifiedException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
