// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Services;

/// <summary>
/// Service interface for generating authentication tokens.
/// Follows Interface Segregation Principle - focused on token generation only.
/// Domain layer defines the contract without knowing implementation details (JWT, OAuth, etc.)
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a token for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="username">The username.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The token string.</returns>
    public Task<string> GenerateTokenAsync(int userId, string username, IEnumerable<string> roles);
}
