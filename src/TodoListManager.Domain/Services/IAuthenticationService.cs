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
    Result<string> Authenticate(string username, string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// Hashes a password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);
}
