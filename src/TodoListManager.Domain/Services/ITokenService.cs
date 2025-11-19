// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Services;

/// <summary>
/// Service interface for generating and validating authentication tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate a token.</param>
    /// <returns>The JWT token string.</returns>
    public Task<string> GenerateTokenAsync(object user);

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The user ID if valid, null otherwise.</returns>
    public int? ValidateToken(string token);
}
