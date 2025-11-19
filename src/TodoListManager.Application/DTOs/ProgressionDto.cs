// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.DTOs;

/// <summary>
/// Data Transfer Object for Progression value object.
/// </summary>
public record ProgressionDto
{
    public DateTime Date { get; init; }
    public decimal Percent { get; init; }
}
