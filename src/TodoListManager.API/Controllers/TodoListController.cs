// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.Services;

namespace TodoListManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoListController : ControllerBase
{
    private readonly TodoListService _todoListService;

    public TodoListController(TodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    [HttpPost("items")]
    public IActionResult AddItem([FromBody] AddItemRequest request)
    {
        var result = _todoListService.AddItem(request.Title, request.Description, request.Category);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Item added successfully" });
    }

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

    [HttpGet("items")]
    public IActionResult GetAllItems()
    {
        var result = _todoListService.GetAllItems();
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value.Items);
    }

    [HttpPost("items/print")]
    public IActionResult PrintItems()
    {
        _todoListService.PrintItems();
        return Ok(new { message = "Items printed to console" });
    }
}

public record AddItemRequest(string Title, string Description, string Category);
public record UpdateItemRequest(string Description);
public record RegisterProgressionRequest(DateTime Date, decimal Percent);
