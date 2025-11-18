// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Common;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;

namespace TodoListManager.Application.Services;

/// <summary>
/// Provides authentication services for verifying user credentials, generating authentication tokens, and managing
/// password hashing and verification operations.
/// </summary>
/// <remarks>This class acts as an application service that orchestrates domain and infrastructure operations
/// related to user authentication. It relies on injected dependencies for user data access, token generation, and
/// password hashing. Thread safety and lifetime management depend on the configuration of the underlying dependencies.
/// Typically, this service is used as part of a login workflow to authenticate users and issue tokens for subsequent
/// requests.</remarks>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        IUserRepository userRepository, 
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Authenticates a user by verifying their credentials and generating an authentication token.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public Result<string> Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<string>("Username is required.");

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<string>("Password is required.");

        var user = _userRepository.GetByUsername(username);
        if (user == null)
            return Result.Failure<string>("Invalid username or password.");

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return Result.Failure<string>("Invalid username or password.");

        var token = _tokenService.GenerateToken(user);
        
        return Result.Success(token);
    }

    /// <summary>
    /// Verifies a password against a hash using domain service.
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        return _passwordHasher.VerifyPassword(password, passwordHash);
    }

    /// <summary>
    /// Hashes a password using domain service.
    /// </summary>
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(password);
    }
}
