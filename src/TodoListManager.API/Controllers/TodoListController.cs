// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Queries;
using TodoListManager.Application.Services;

namespace TodoListManager.API.Controllers;

/// <summary>
/// API controller for managing todo list items.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TodoListController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly TodoListPresentationService _presentationService;

    public TodoListController(IMediator mediator, TodoListPresentationService presentationService)
    {
        _mediator = mediator;
        _presentationService = presentationService;
    }

    /// <summary>
    /// Adds a new todo item.
    /// </summary>
    /// <param name="request">The item details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpPost("items")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddTodoItemCommand(request.Title, request.Description, request.Category);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Item added successfully" });
    }

    /// <summary>
    /// Updates the description of an existing todo item.
    /// </summary>
    /// <param name="id">The item ID.</param>
    /// <param name="request">The updated description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpPut("items/{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTodoItemCommand(id, request.Description);
        var result = await _mediator.Send(command, cancellationToken);
        
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpDelete("items/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveItem(int id, CancellationToken cancellationToken)
    {
        var command = new RemoveTodoItemCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpPost("items/{id}/progressions")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> RegisterProgression(int id, [FromBody] RegisterProgressionRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterProgressionCommand(id, request.Date, request.Percent);
        var result = await _mediator.Send(command, cancellationToken);
        
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all todo items.</returns>
    [HttpGet("items")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetAllItems(CancellationToken cancellationToken)
    {
        var query = new GetAllTodoItemsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value.Items);
    }

    /// <summary>
    /// Prints all todo items to the console (for debugging).
    /// </summary>
    /// <returns>Success message.</returns>
    [HttpPost("items/print")]
    [Authorize(Roles = "Admin")]
    public IActionResult PrintItems()
    {
        _presentationService.PrintItems();
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
