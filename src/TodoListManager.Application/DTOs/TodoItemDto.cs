// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.DTOs;

/// <summary>
/// Data Transfer Object for TodoItem entity.
/// Represents the public API contract for todo items.
/// </summary>
public record TodoItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal TotalProgress { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? LastProgressionDate { get; init; }
    public List<ProgressionDto> Progressions { get; init; } = new();
}
