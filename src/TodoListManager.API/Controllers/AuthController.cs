// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.DTOs;
using TodoListManager.Domain.Services;
using TodoListManager.Infrastructure.Identity;

namespace TodoListManager.API.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        IAuthenticationService authenticationService,
        UserManager<ApplicationUser> userManager)
    {
        _authenticationService = authenticationService;
        _userManager = userManager;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT token and user information.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authenticationService.Authenticate(request.Username, request.Password);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
            return Unauthorized(new { error = "User not found" });

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var response = new LoginResponse
        {
            Token = result.Value,
            Username = user.UserName ?? string.Empty,
            Roles = roles.ToList(),
            Permissions = claims.Select(c => c.Value).Distinct().ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>User information.</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound(new { error = "User not found" });

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        return Ok(new
        {
            user.Id,
            Username = user.UserName,
            Email = user.Email,
            Roles = roles.ToList(),
            Permissions = claims.Select(c => c.Value).Distinct().ToList()
        });
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <returns>Result of registration.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { error = errors });
        }

        // Assign default User role
        await _userManager.AddToRoleAsync(user, "User");

        return Ok(new { message = "User registered successfully" });
    }
}

/// <summary>
/// Request model for user registration.
/// </summary>
/// <param name="Username">The username.</param>
/// <param name="Email">The email address.</param>
/// <param name="Password">The password.</param>
public record RegisterRequest(string Username, string Email, string Password);
