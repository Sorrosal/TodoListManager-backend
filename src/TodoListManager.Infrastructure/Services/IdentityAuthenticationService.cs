// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Identity;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Services;
using TodoListManager.Infrastructure.Identity;

namespace TodoListManager.Infrastructure.Services;

/// <summary>
/// Authentication service using ASP.NET Core Identity.
/// Infrastructure concern - implements domain interface.
/// </summary>
public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public IdentityAuthenticationService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<Result<string>> Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<string>("Username is required.");

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<string>("Password is required.");

        var identityUser = await _userManager.FindByNameAsync(username);
        if (identityUser == null)
            return Result.Failure<string>("Invalid username or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, password);
        if (!isPasswordValid)
            return Result.Failure<string>("Invalid username or password.");

        var roles = await _userManager.GetRolesAsync(identityUser);
        var token = await _tokenService.GenerateTokenAsync(
            identityUser.Id,
            identityUser.UserName ?? string.Empty,
            roles);

        return Result.Success(token);
    }
}
