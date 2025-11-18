// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Queries;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the query to retrieve all todo items.
/// </summary>
public class GetAllTodoItemsQueryHandler : IRequestHandler<GetAllTodoItemsQuery, Result<GetAllTodoItemsQueryResult>>
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing all todo items or an error.</returns>
    public Task<Result<GetAllTodoItemsQueryResult>> Handle(GetAllTodoItemsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var items = _todoList.GetAllItems().ToList();
            var result = new GetAllTodoItemsQueryResult(items);
            return Task.FromResult(Result.Success(result));
        }
        catch (DomainException ex)
        {
            return Task.FromResult(Result.Failure<GetAllTodoItemsQueryResult>(ex.Message));
        }
    }
}
