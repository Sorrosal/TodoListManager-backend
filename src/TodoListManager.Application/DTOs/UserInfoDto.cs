// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.DTOs;

/// <summary>
/// DTO for user information.
/// Used to transfer user data between layers without exposing domain entities.
/// </summary>
public record UserInfoDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public List<string> Permissions { get; init; } = new();
}
