// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Queries;

namespace TodoListManager.API.Controllers;

/// <summary>
/// API controller for managing todo list items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoListController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of <see cref="TodoListController"/>.
    /// </summary>
    /// <param name="mediator">The MediatR instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when mediator is null.</exception>
    public TodoListController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Adds a new todo item.
    /// </summary>
    /// <param name="request">The item details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpPost("items")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpPut("items/{id}")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpDelete("items/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or error message.</returns>
    [HttpPost("items/{id}/progressions")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A response containing all todo items as DTOs.</returns>
    [HttpGet("items")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllItems(CancellationToken cancellationToken)
    {
        var query = new GetAllTodoItemsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}

/// <summary>
/// Request model for adding a new todo item.
/// </summary>
/// <param name="Title">The title of the item.</param>
/// <param name="Description">The description of the item.</param>
/// <param name="Category">The category of the item.</param>
public record AddItemRequest(string Title, string Description, string Category);

/// <summary>
/// Request model for updating a todo item.
/// </summary>
/// <param name="Description">The new description.</param>
public record UpdateItemRequest(string Description);

/// <summary>
/// Request model for registering a progression.
/// </summary>
/// <param name="Date">The progression date.</param>
/// <param name="Percent">The percentage of progress.</param>
public record RegisterProgressionRequest(DateTime Date, decimal Percent);
