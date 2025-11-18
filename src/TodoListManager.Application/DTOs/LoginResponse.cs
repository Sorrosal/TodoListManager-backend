// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.DTOs;

/// <summary>
/// Response model for successful login.
/// </summary>
public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public List<string> Permissions { get; init; } = new();
}
