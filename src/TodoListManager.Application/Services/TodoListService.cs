// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Commands;
using TodoListManager.Application.Handlers;
using TodoListManager.Application.Queries;
using TodoListManager.Domain.Aggregates;

namespace TodoListManager.Application.Services;

public class TodoListService
{
    private readonly ITodoList _todoList;
    private readonly AddTodoItemCommandHandler _addHandler;
    private readonly UpdateTodoItemCommandHandler _updateHandler;
    private readonly RemoveTodoItemCommandHandler _removeHandler;
    private readonly RegisterProgressionCommandHandler _registerProgressionHandler;
    private readonly GetAllTodoItemsQueryHandler _getAllHandler;

    public TodoListService(
        ITodoList todoList,
        AddTodoItemCommandHandler addHandler,
        UpdateTodoItemCommandHandler updateHandler,
        RemoveTodoItemCommandHandler removeHandler,
        RegisterProgressionCommandHandler registerProgressionHandler,
        GetAllTodoItemsQueryHandler getAllHandler)
    {
        _todoList = todoList;
        _addHandler = addHandler;
        _updateHandler = updateHandler;
        _removeHandler = removeHandler;
        _registerProgressionHandler = registerProgressionHandler;
        _getAllHandler = getAllHandler;
    }

    public void AddItem(string title, string description, string category)
    {
        var command = new AddTodoItemCommand(title, description, category);
        _addHandler.Handle(command);
    }

    public void UpdateItem(int id, string description)
    {
        var command = new UpdateTodoItemCommand(id, description);
        _updateHandler.Handle(command);
    }

    public void RemoveItem(int id)
    {
        var command = new RemoveTodoItemCommand(id);
        _removeHandler.Handle(command);
    }

    public void RegisterProgression(int id, DateTime date, decimal percent)
    {
        var command = new RegisterProgressionCommand(id, date, percent);
        _registerProgressionHandler.Handle(command);
    }

    public void PrintItems()
    {
        _todoList.PrintItems();
    }

    public GetAllTodoItemsQueryResult GetAllItems()
    {
        var query = new GetAllTodoItemsQuery();
        return _getAllHandler.Handle(query);
    }
}
