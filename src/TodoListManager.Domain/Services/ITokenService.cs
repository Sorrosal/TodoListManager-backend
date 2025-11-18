// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;

namespace TodoListManager.Domain.Services;

/// <summary>
/// Service interface for generating and validating authentication tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>The JWT token string.</returns>
    string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The user ID if valid, null otherwise.</returns>
    int? ValidateToken(string token);
}
