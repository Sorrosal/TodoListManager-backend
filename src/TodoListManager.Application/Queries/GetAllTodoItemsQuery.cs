// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Entities;

namespace TodoListManager.Application.Queries;

public record GetAllTodoItemsQuery : IRequest<Result<GetAllTodoItemsQueryResult>>;

public record GetAllTodoItemsQueryResult(List<TodoItem> Items);
