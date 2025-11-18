// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to register a progression for a todo item.
/// </summary>
public class RegisterProgressionCommandHandler
{
    private readonly ITodoList _todoList;

    public RegisterProgressionCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    /// <summary>
    /// Handles the register progression command.
    /// </summary>
    /// <param name="command">The command containing progression details.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Result Handle(RegisterProgressionCommand command)
    {
        try
        {
            _todoList.RegisterProgression(command.Id, command.Date, command.Percent);
            return Result.Success();
        }
        catch (TodoItemNotFoundException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (InvalidProgressionException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
