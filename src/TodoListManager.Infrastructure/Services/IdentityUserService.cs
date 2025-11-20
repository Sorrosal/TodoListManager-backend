// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Identity;
using TodoListManager.Application.DTOs;
using TodoListManager.Application.Services;
using TodoListManager.Domain.Common;
using TodoListManager.Infrastructure.Identity;

namespace TodoListManager.Infrastructure.Services;

/// <summary>
/// Implementation of IUserService using ASP.NET Core Identity.
/// Infrastructure concern - bridges domain/application to Identity framework.
/// </summary>
public class IdentityUserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<Result<UserInfoDto>> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<UserInfoDto>("Username is required.");

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return Result.Failure<UserInfoDto>("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var userInfo = new UserInfoDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            Permissions = claims.Select(c => c.Value).Distinct().ToList()
        };

        return Result.Success(userInfo);
    }

    public async Task<Result> RegisterUserAsync(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure("Username is required.");

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure("Email is required.");

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure("Password is required.");

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(errors);
        }

        // Assign default User role
        await _userManager.AddToRoleAsync(user, "User");

        return Result.Success();
    }
}
