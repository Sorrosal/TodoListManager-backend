// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to update an existing todo item.
/// </summary>
public class UpdateTodoItemCommandHandler
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
    /// <returns>A result indicating success or failure.</returns>
    public Result Handle(UpdateTodoItemCommand command)
    {
        try
        {
            _todoList.UpdateItem(command.Id, command.Description);
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
