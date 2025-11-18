// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Mvc;
using TodoListManager.Application.Services;
using TodoListManager.Domain.Exceptions;

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
        try
        {
            _todoListService.AddItem(request.Title, request.Description, request.Category);
            return Ok(new { message = "Item added successfully" });
        }
        catch (InvalidCategoryException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("items/{id}")]
    public IActionResult UpdateItem(int id, [FromBody] UpdateItemRequest request)
    {
        try
        {
            _todoListService.UpdateItem(id, request.Description);
            return Ok(new { message = "Item updated successfully" });
        }
        catch (TodoItemNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (TodoItemCannotBeModifiedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("items/{id}")]
    public IActionResult RemoveItem(int id)
    {
        try
        {
            _todoListService.RemoveItem(id);
            return Ok(new { message = "Item removed successfully" });
        }
        catch (TodoItemNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (TodoItemCannotBeModifiedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("items/{id}/progressions")]
    public IActionResult RegisterProgression(int id, [FromBody] RegisterProgressionRequest request)
    {
        try
        {
            _todoListService.RegisterProgression(id, request.Date, request.Percent);
            return Ok(new { message = "Progression registered successfully" });
        }
        catch (TodoItemNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidProgressionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("items")]
    public IActionResult GetAllItems()
    {
        try
        {
            var result = _todoListService.GetAllItems();
            return Ok(result.Items);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("items/print")]
    public IActionResult PrintItems()
    {
        try
        {
            _todoListService.PrintItems();
            return Ok(new { message = "Items printed to console" });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record AddItemRequest(string Title, string Description, string Category);
public record UpdateItemRequest(string Description);
public record RegisterProgressionRequest(DateTime Date, decimal Percent);
