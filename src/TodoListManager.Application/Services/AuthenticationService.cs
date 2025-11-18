// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Common;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;

namespace TodoListManager.Application.Services;

/// <summary>
/// Implementation of authentication service.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthenticationService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public Result<string> Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<string>("Username is required.");

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<string>("Password is required.");

        var user = _userRepository.GetByUsername(username);
        if (user == null)
            return Result.Failure<string>("Invalid username or password.");

        if (!VerifyPassword(password, user.PasswordHash))
            return Result.Failure<string>("Invalid username or password.");

        var token = _tokenService.GenerateToken(user);
        return Result.Success(token);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
