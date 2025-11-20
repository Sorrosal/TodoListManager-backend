// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.DTOs;
using TodoListManager.Domain.Common;

namespace TodoListManager.Application.Services;

/// <summary>
/// Application service for user management operations.
/// Abstracts infrastructure concerns from the API layer.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets user information by username.
    /// </summary>
    /// <param name="username">The username to look up.</param>
    /// <returns>A result containing user information if found.</returns>
    public Task<Result<UserInfoDto>> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="email">The email address.</param>
    /// <param name="password">The password.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Task<Result> RegisterUserAsync(string username, string email, string password);
}
