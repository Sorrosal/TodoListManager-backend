// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Handlers;
using TodoListManager.Application.Queries;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;

namespace TodoListManager.Application.Services;

public class TodoListService
{
    private readonly ITodoList _todoList;
    private readonly AddTodoItemCommandHandler _addHandler;
    private readonly UpdateTodoItemCommandHandler _updateHandler;
    private readonly RemoveTodoItemCommandHandler _removeHandler;
    private readonly RegisterProgressionCommandHandler _registerProgressionHandler;
    private readonly GetAllTodoItemsQueryHandler _getAllHandler;
    private readonly IValidator<AddTodoItemCommand> _addValidator;
    private readonly IValidator<UpdateTodoItemCommand> _updateValidator;
    private readonly IValidator<RemoveTodoItemCommand> _removeValidator;
    private readonly IValidator<RegisterProgressionCommand> _registerProgressionValidator;

    public TodoListService(
        ITodoList todoList,
        AddTodoItemCommandHandler addHandler,
        UpdateTodoItemCommandHandler updateHandler,
        RemoveTodoItemCommandHandler removeHandler,
        RegisterProgressionCommandHandler registerProgressionHandler,
        GetAllTodoItemsQueryHandler getAllHandler,
        IValidator<AddTodoItemCommand> addValidator,
        IValidator<UpdateTodoItemCommand> updateValidator,
        IValidator<RemoveTodoItemCommand> removeValidator,
        IValidator<RegisterProgressionCommand> registerProgressionValidator)
    {
        _todoList = todoList;
        _addHandler = addHandler;
        _updateHandler = updateHandler;
        _removeHandler = removeHandler;
        _registerProgressionHandler = registerProgressionHandler;
        _getAllHandler = getAllHandler;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
        _removeValidator = removeValidator;
        _registerProgressionValidator = registerProgressionValidator;
    }

    public Result AddItem(string title, string description, string category)
    {
        var command = new AddTodoItemCommand(title, description, category);
        
        var validationResult = _addValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        return _addHandler.Handle(command);
    }

    public Result UpdateItem(int id, string description)
    {
        var command = new UpdateTodoItemCommand(id, description);
        
        var validationResult = _updateValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        return _updateHandler.Handle(command);
    }

    public Result RemoveItem(int id)
    {
        var command = new RemoveTodoItemCommand(id);
        
        var validationResult = _removeValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        return _removeHandler.Handle(command);
    }

    public Result RegisterProgression(int id, DateTime date, decimal percent)
    {
        var command = new RegisterProgressionCommand(id, date, percent);
        
        var validationResult = _registerProgressionValidator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        return _registerProgressionHandler.Handle(command);
    }

    public void PrintItems()
    {
        _todoList.PrintItems();
    }

    public Result<GetAllTodoItemsQueryResult> GetAllItems()
    {
        var query = new GetAllTodoItemsQuery();
        return _getAllHandler.Handle(query);
    }
}
