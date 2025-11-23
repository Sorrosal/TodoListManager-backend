// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Handlers;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Specifications;

namespace TodoListManager.Application.Tests.Handlers;

/// <summary>
/// Unit tests for AddTodoItemCommandHandler following best practices.
/// Updated to use repository + unit of work.
/// </summary>
public class AddTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoListRepository> _mockRepository;
    private readonly Mock<TodoListManager.Domain.Common.IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICategoryValidator> _mockCategoryValidator;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;
    private readonly AddTodoItemCommandHandler _handler;

    public AddTodoItemCommandHandlerTests()
    {
        _mockRepository = new Mock<ITodoListRepository>();
        _mockUnitOfWork = new Mock<TodoListManager.Domain.Common.IUnitOfWork>();
        _mockCategoryValidator = new Mock<ICategoryValidator>();
        _mockCategoryValidator.Setup(v => v.IsValidCategory(It.IsAny<string>())).Returns(true);
        _canModifySpecification = new CanModifyTodoItemSpecification();
        _validProgressionSpecification = new ValidProgressionSpecification();
        _handler = new AddTodoItemCommandHandler(
            _mockRepository.Object, 
            _mockUnitOfWork.Object, 
            _mockCategoryValidator.Object,
            _canModifySpecification,
            _validProgressionSpecification);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var handler = new AddTodoItemCommandHandler(
            _mockRepository.Object, 
            _mockUnitOfWork.Object, 
            _mockCategoryValidator.Object,
            _canModifySpecification,
            _validProgressionSpecification);

        // Assert
        handler.Should().NotBeNull();
    }

    #endregion

    #region Handle - Success Tests

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new AddTodoItemCommand("Test Task", "Description", "Work");

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockCategoryValidator.Verify(x => x.IsValidCategory("Work"), Times.Once);
        _mockRepository.Verify(x => x.SaveAsync(It.Is<TodoItem>(i => i.Id == 0 && i.Title == "Test Task"), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryInCorrectOrder()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Desc", "Work");
        var callOrder = new List<string>();

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Save"))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert order
        callOrder.Should().ContainInOrder("Save", "SaveChanges");
    }

    [Fact]
    public async Task Handle_WithMultipleCommands_ShouldCreateMultipleItems()
    {
        // Arrange
        var command1 = new AddTodoItemCommand("Task 1", "Desc 1", "Work");
        var command2 = new AddTodoItemCommand("Task 2", "Desc 2", "Personal");

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert - both items should be created with temporary ID 0
        _mockRepository.Verify(x => x.SaveAsync(It.Is<TodoItem>(i => i.Id == 0 && i.Title == "Task 1"), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveAsync(It.Is<TodoItem>(i => i.Id == 0 && i.Title == "Task 2"), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Handle - Failure Tests

    [Fact]
    public async Task Handle_WithInvalidCategory_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "InvalidCategory");

        _mockCategoryValidator.Setup(v => v.IsValidCategory("InvalidCategory")).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("InvalidCategory");
    }

    [Fact]
    public async Task Handle_WhenSaveThrowsDomainException_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "Work");

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .Throws(new DomainException("Domain error occurred"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Domain error occurred");
    }

    [Fact]
    public async Task Handle_WhenGetAggregateThrowsException_ShouldThrow()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "Work");

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassItThrough()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "Work");
        
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.Handle(command, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<TodoItem>(), cts.Token), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithEmptyStrings_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddTodoItemCommand("", "", "Work");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should fail because domain validates non-empty title
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Title");
    }

    [Fact]
    public async Task Handle_WithLongStrings_ShouldHandleCorrectly()
    {
        // Arrange
        var longTitle = new string('A', 1000);
        var longDescription = new string('B', 5000);
        var command = new AddTodoItemCommand(longTitle, longDescription, "Work");
        
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.SaveAsync(
            It.Is<TodoItem>(i => i.Title == longTitle && i.Description == longDescription), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task Handle_CompleteSuccessWorkflow_ShouldExecuteAllSteps()
    {
        // Arrange
        var command = new AddTodoItemCommand(
            "Complete Project Documentation",
            "Write comprehensive documentation for the project",
            "Work"
        );
        
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEmpty();

        _mockCategoryValidator.Verify(x => x.IsValidCategory("Work"), Times.Once);
        _mockRepository.Verify(x => x.SaveAsync(
            It.Is<TodoItem>(i => 
                i.Id == 0 && 
                i.Title == "Complete Project Documentation" &&
                i.Description == "Write comprehensive documentation for the project" &&
                i.Category == "Work"), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkFails_ShouldNotAffectResult()
    {
        // Arrange - simulating a scenario where SaveChanges returns 0 (no rows affected)
        var command = new AddTodoItemCommand("Task", "Description", "Work");
        
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler doesn't check the return value, so it still succeeds
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}





















