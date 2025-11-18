// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.DTOs;

/// <summary>
/// Request model for user login.
/// </summary>
public record LoginRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
