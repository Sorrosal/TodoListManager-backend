// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.DTOs;

/// <summary>
/// Response model for GetAllTodoItems query.
/// </summary>
public record GetAllTodoItemsResponse
{
    public List<TodoItemDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
}
