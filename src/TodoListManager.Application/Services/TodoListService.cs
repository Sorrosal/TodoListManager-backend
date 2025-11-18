// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Handlers;
using TodoListManager.Application.Queries;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;

namespace TodoListManager.Application.Services;

/// <summary>
/// Application service that coordinates todo list operations with validation.
/// </summary>
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
    private readonly TodoListPresentationService _presentationService;

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
        IValidator<RegisterProgressionCommand> registerProgressionValidator,
        TodoListPresentationService presentationService)
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
        _presentationService = presentationService;
    }

    /// <summary>
    /// Adds a new todo item with validation.
    /// </summary>
    /// <param name="title">The title of the item.</param>
    /// <param name="description">The description of the item.</param>
    /// <param name="category">The category of the item.</param>
    /// <returns>A result indicating success or failure.</returns>
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

    /// <summary>
    /// Updates an existing todo item with validation.
    /// </summary>
    /// <param name="id">The unique identifier of the item to update.</param>
    /// <param name="description">The new description.</param>
    /// <returns>A result indicating success or failure.</returns>
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

    /// <summary>
    /// Removes a todo item with validation.
    /// </summary>
    /// <param name="id">The unique identifier of the item to remove.</param>
    /// <returns>A result indicating success or failure.</returns>
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

    /// <summary>
    /// Registers a progression entry for a todo item with validation.
    /// </summary>
    /// <param name="id">The unique identifier of the item.</param>
    /// <param name="date">The date of the progression.</param>
    /// <param name="percent">The percentage of progress to add.</param>
    /// <returns>A result indicating success or failure.</returns>
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

    /// <summary>
    /// Prints all todo items to the console.
    /// </summary>
    public void PrintItems()
    {
        _presentationService.PrintItems();
    }

    /// <summary>
    /// Gets all todo items.
    /// </summary>
    /// <returns>A result containing all todo items or an error.</returns>
    public Result<GetAllTodoItemsQueryResult> GetAllItems()
    {
        var query = new GetAllTodoItemsQuery();
        return _getAllHandler.Handle(query);
    }
}
