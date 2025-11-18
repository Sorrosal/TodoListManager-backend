// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.DTOs;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;

namespace TodoListManager.API.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthenticationService authenticationService, IUserRepository userRepository)
    {
        _authenticationService = authenticationService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT token and user information.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _authenticationService.Authenticate(request.Username, request.Password);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        var user = _userRepository.GetByUsername(request.Username);
        if (user == null)
            return Unauthorized(new { error = "User not found" });

        var response = new LoginResponse
        {
            Token = result.Value,
            Username = user.Username,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Permissions = user.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Name)
                .Distinct()
                .ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>User information.</returns>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var user = _userRepository.GetByUsername(username);
        if (user == null)
            return NotFound(new { error = "User not found" });

        return Ok(new
        {
            user.Id,
            user.Username,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Permissions = user.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Name)
                .Distinct()
                .ToList()
        });
    }
}
