// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Queries;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the query to retrieve all todo items.
/// </summary>
public class GetAllTodoItemsQueryHandler
{
    private readonly TodoList _todoList;

    public GetAllTodoItemsQueryHandler(TodoList todoList)
    {
        _todoList = todoList;
    }

    /// <summary>
    /// Handles the get all todo items query.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <returns>A result containing all todo items or an error.</returns>
    public Result<GetAllTodoItemsQueryResult> Handle(GetAllTodoItemsQuery query)
    {
        try
        {
            var items = _todoList.GetAllItems().ToList();
            var result = new GetAllTodoItemsQueryResult(items);
            return Result.Success(result);
        }
        catch (DomainException ex)
        {
            return Result.Failure<GetAllTodoItemsQueryResult>(ex.Message);
        }
    }
}
