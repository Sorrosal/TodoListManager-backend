// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;

namespace TodoListManager.Application.Queries;

public record GetAllTodoItemsQuery;

public record GetAllTodoItemsQueryResult(List<TodoItem> Items);
