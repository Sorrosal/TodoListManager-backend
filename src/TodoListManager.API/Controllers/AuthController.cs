// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.DTOs;
using TodoListManager.Application.Services;
using TodoListManager.Domain.Services;

namespace TodoListManager.API.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserService _userService;

    public AuthController(
        IAuthenticationService authenticationService,
        IUserService userService)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT token and user information.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResult = await _authenticationService.Authenticate(request.Username, request.Password);

        if (authResult.IsFailure)
            return Unauthorized(new { error = authResult.Error });

        var userResult = await _userService.GetUserByUsernameAsync(request.Username);

        if (userResult.IsFailure)
            return Unauthorized(new { error = userResult.Error });

        var userInfo = userResult.Value;
        var response = new LoginResponse
        {
            Token = authResult.Value,
            Username = userInfo.Username,
            Roles = userInfo.Roles,
            Permissions = userInfo.Permissions
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>User information.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var result = await _userService.GetUserByUsernameAsync(username);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        var userInfo = result.Value;
        return Ok(new
        {
            userInfo.Id,
            userInfo.Username,
            userInfo.Email,
            userInfo.Roles,
            userInfo.Permissions
        });
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <returns>Result of registration.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterUserAsync(
            request.Username, 
            request.Email, 
            request.Password);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

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
