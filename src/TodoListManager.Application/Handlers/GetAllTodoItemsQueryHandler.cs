// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Queries;
using TodoListManager.Domain.Aggregates;

namespace TodoListManager.Application.Handlers;

public class GetAllTodoItemsQueryHandler
{
    private readonly TodoList _todoList;

    public GetAllTodoItemsQueryHandler(TodoList todoList)
    {
        _todoList = todoList;
    }

    public GetAllTodoItemsQueryResult Handle(GetAllTodoItemsQuery query)
    {
        var items = _todoList.GetAllItems().ToList();
        return new GetAllTodoItemsQueryResult(items);
    }
}
