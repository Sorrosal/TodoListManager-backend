// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using FluentValidation.TestHelper;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Validators;

namespace TodoListManager.Application.Tests.Validators;

public class AddTodoItemCommandValidatorTests
{
    private readonly AddTodoItemCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new AddTodoItemCommand("Task", "Description", "Work");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidTitle_ShouldHaveError(string title)
    {
        var command = new AddTodoItemCommand(title, "Description", "Work");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidDescription_ShouldHaveError(string description)
    {
        var command = new AddTodoItemCommand("Task", description, "Work");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidCategory_ShouldHaveError(string category)
    {
        var command = new AddTodoItemCommand("Task", "Description", category);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }
}

public class UpdateTodoItemCommandValidatorTests
{
    private readonly UpdateTodoItemCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new UpdateTodoItemCommand(1, "New Description");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidId_ShouldHaveError(int id)
    {
        var command = new UpdateTodoItemCommand(id, "Description");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidDescription_ShouldHaveError(string description)
    {
        var command = new UpdateTodoItemCommand(1, description);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}

public class RemoveTodoItemCommandValidatorTests
{
    private readonly RemoveTodoItemCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_ShouldNotHaveErrors()
    {
        var command = new RemoveTodoItemCommand(1);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidId_ShouldHaveError(int id)
    {
        var command = new RemoveTodoItemCommand(id);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}

public class RegisterProgressionCommandValidatorTests
{
    private readonly RegisterProgressionCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new RegisterProgressionCommand(1, DateTime.Now.AddMinutes(-1), 25m);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidId_ShouldHaveError(int id)
    {
        var command = new RegisterProgressionCommand(id, DateTime.Now.AddMinutes(-1), 25m);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithInvalidPercent_ShouldHaveError(decimal percent)
    {
        var command = new RegisterProgressionCommand(1, DateTime.Now.AddMinutes(-1), percent);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Percent);
    }

    [Fact]
    public void Validate_WithFutureDate_ShouldHaveError()
    {
        var command = new RegisterProgressionCommand(1, DateTime.Now.AddDays(1), 25m);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }
}
