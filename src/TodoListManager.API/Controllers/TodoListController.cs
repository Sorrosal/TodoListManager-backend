// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.Services;

namespace TodoListManager.API.Controllers;

/// <summary>
/// API controller for managing todo list items.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TodoListController : ControllerBase
{
    private readonly TodoListService _todoListService;

    public TodoListController(TodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    /// <summary>
    /// Adds a new todo item.
    /// </summary>
    /// <param name="request">The item details.</param>
    /// <returns>Success or error message.</returns>
    [HttpPost("items")]
    public IActionResult AddItem([FromBody] AddItemRequest request)
    {
        var result = _todoListService.AddItem(request.Title, request.Description, request.Category);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Item added successfully" });
    }

    /// <summary>
    /// Updates the description of an existing todo item.
    /// </summary>
    /// <param name="id">The item ID.</param>
    /// <param name="request">The updated description.</param>
    /// <returns>Success or error message.</returns>
    [HttpPut("items/{id}")]
    public IActionResult UpdateItem(int id, [FromBody] UpdateItemRequest request)
    {
        var result = _todoListService.UpdateItem(id, request.Description);
        
        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = result.Error });
            
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Item updated successfully" });
    }

    /// <summary>
    /// Removes a todo item.
    /// </summary>
    /// <param name="id">The item ID to remove.</param>
    /// <returns>Success or error message.</returns>
    [HttpDelete("items/{id}")]
    public IActionResult RemoveItem(int id)
    {
        var result = _todoListService.RemoveItem(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = result.Error });
            
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Item removed successfully" });
    }

    /// <summary>
    /// Registers a progression entry for a todo item.
    /// </summary>
    /// <param name="id">The item ID.</param>
    /// <param name="request">The progression details.</param>
    /// <returns>Success or error message.</returns>
    [HttpPost("items/{id}/progressions")]
    public IActionResult RegisterProgression(int id, [FromBody] RegisterProgressionRequest request)
    {
        var result = _todoListService.RegisterProgression(id, request.Date, request.Percent);
        
        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = result.Error });
            
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Progression registered successfully" });
    }

    /// <summary>
    /// Gets all todo items.
    /// </summary>
    /// <returns>A list of all todo items.</returns>
    [HttpGet("items")]
    public IActionResult GetAllItems()
    {
        var result = _todoListService.GetAllItems();
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value.Items);
    }

    /// <summary>
    /// Prints all todo items to the console (for debugging).
    /// </summary>
    /// <returns>Success message.</returns>
    [HttpPost("items/print")]
    public IActionResult PrintItems()
    {
        _todoListService.PrintItems();
        return Ok(new { message = "Items printed to console" });
    }
}

/// <summary>
/// Request model for adding a new todo item.
/// </summary>
public record AddItemRequest(string Title, string Description, string Category);

/// <summary>
/// Request model for updating a todo item.
/// </summary>
public record UpdateItemRequest(string Description);

/// <summary>
/// Request model for registering a progression.
/// </summary>
public record RegisterProgressionRequest(DateTime Date, decimal Percent);
