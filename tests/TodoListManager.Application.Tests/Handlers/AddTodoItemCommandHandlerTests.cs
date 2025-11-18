// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Handlers;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Tests.Handlers;

/// <summary>
/// Unit tests for AddTodoItemCommandHandler following best practices.
/// </summary>
public class AddTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoList> _mockTodoList;
    private readonly Mock<ITodoListRepository> _mockRepository;
    private readonly AddTodoItemCommandHandler _handler;

    public AddTodoItemCommandHandlerTests()
    {
        _mockTodoList = new Mock<ITodoList>();
        _mockRepository = new Mock<ITodoListRepository>();
        _handler = new AddTodoItemCommandHandler(_mockTodoList.Object, _mockRepository.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var handler = new AddTodoItemCommandHandler(_mockTodoList.Object, _mockRepository.Object);

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
        _mockRepository.Setup(x => x.GetNextId()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.GetNextId(), Times.Once);
        _mockTodoList.Verify(x => x.AddItem(1, "Test Task", "Description", "Work"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryBeforeAddingItem()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Desc", "Category");
        var callOrder = new List<string>();

        _mockRepository.Setup(x => x.GetNextId())
            .Returns(5)
            .Callback(() => callOrder.Add("GetNextId"));

        _mockTodoList.Setup(x => x.AddItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => callOrder.Add("AddItem"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("GetNextId", "AddItem");
    }

    [Fact]
    public async Task Handle_WithMultipleCommands_ShouldUseCorrectIds()
    {
        // Arrange
        var command1 = new AddTodoItemCommand("Task 1", "Desc 1", "Work");
        var command2 = new AddTodoItemCommand("Task 2", "Desc 2", "Personal");

        _mockRepository.SetupSequence(x => x.GetNextId())
            .Returns(1)
            .Returns(2);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        _mockTodoList.Verify(x => x.AddItem(1, "Task 1", "Desc 1", "Work"), Times.Once);
        _mockTodoList.Verify(x => x.AddItem(2, "Task 2", "Desc 2", "Personal"), Times.Once);
    }

    #endregion

    #region Handle - Failure Tests

    [Fact]
    public async Task Handle_WithInvalidCategory_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "InvalidCategory");
        _mockRepository.Setup(x => x.GetNextId()).Returns(1);
        _mockTodoList.Setup(x => x.AddItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), "InvalidCategory"))
            .Throws(new InvalidCategoryException("InvalidCategory"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("InvalidCategory");
    }

    [Fact]
    public async Task Handle_WhenAddItemThrowsDomainException_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "Work");
        var errorMessage = "Domain error occurred";
        _mockRepository.Setup(x => x.GetNextId()).Returns(1);
        _mockTodoList.Setup(x => x.AddItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new DomainException(errorMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldNotPropagateUnhandledException()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "Work");
        _mockRepository.Setup(x => x.GetNextId()).Returns(1);
        _mockTodoList.Setup(x => x.AddItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new InvalidCategoryException("test"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var command = new AddTodoItemCommand("Task", "Description", "Work");
        _mockRepository.Setup(x => x.GetNextId()).Returns(1);
        var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.Handle(command, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithEmptyStrings_ShouldStillCallAddItem()
    {
        // Arrange
        var command = new AddTodoItemCommand("", "", "");
        _mockRepository.Setup(x => x.GetNextId()).Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockTodoList.Verify(x => x.AddItem(1, "", "", ""), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSpecialCharacters_ShouldPassThrough()
    {
        // Arrange
        var command = new AddTodoItemCommand(
            "Task with !@#$%",
            "Description with <html>",
            "Work & Life"
        );
        _mockRepository.Setup(x => x.GetNextId()).Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockTodoList.Verify(x => x.AddItem(
            1,
            "Task with !@#$%",
            "Description with <html>",
            "Work & Life"
        ), Times.Once);
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
        var nextId = 42;

        _mockRepository.Setup(x => x.GetNextId()).Returns(nextId);
        _mockTodoList.Setup(x => x.AddItem(
            nextId,
            command.Title,
            command.Description,
            command.Category
        ));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEmpty();

        // Verify all interactions in correct order
        _mockRepository.Verify(x => x.GetNextId(), Times.Once);
        _mockTodoList.Verify(x => x.AddItem(
            nextId,
            command.Title,
            command.Description,
            command.Category
        ), Times.Once);

        // Verify no other interactions
        _mockRepository.VerifyNoOtherCalls();
    }

    #endregion
}
