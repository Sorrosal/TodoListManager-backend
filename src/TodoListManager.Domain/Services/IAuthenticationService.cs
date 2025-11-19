// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Common;

namespace TodoListManager.Domain.Services;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>A result containing the JWT token if successful.</returns>
    Task<Result<string>> Authenticate(string username, string password);
}
