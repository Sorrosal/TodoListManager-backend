// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Application.Commands;
using TodoListManager.Application.Handlers;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Exceptions;

namespace TodoListManager.Application.Tests.Handlers;

public class UpdateTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoList> _mockTodoList;
    private readonly UpdateTodoItemCommandHandler _handler;

    public UpdateTodoItemCommandHandlerTests()
    {
        _mockTodoList = new Mock<ITodoList>();
        _handler = new UpdateTodoItemCommandHandler(_mockTodoList.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(1, "Updated Description");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockTodoList.Verify(x => x.UpdateItem(1, "Updated Description"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(999, "Description");
        _mockTodoList.Setup(x => x.UpdateItem(999, It.IsAny<string>()))
            .Throws(new TodoItemNotFoundException(999));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("999");
    }

    [Fact]
    public async Task Handle_WithItemOverThreshold_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(1, "Description");
        _mockTodoList.Setup(x => x.UpdateItem(1, It.IsAny<string>()))
            .Throws(new TodoItemCannotBeModifiedException(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDomainException_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(1, "Description");
        _mockTodoList.Setup(x => x.UpdateItem(It.IsAny<int>(), It.IsAny<string>()))
            .Throws(new DomainException("Domain error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Domain error");
    }
}

public class RemoveTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoList> _mockTodoList;
    private readonly RemoveTodoItemCommandHandler _handler;

    public RemoveTodoItemCommandHandlerTests()
    {
        _mockTodoList = new Mock<ITodoList>();
        _handler = new RemoveTodoItemCommandHandler(_mockTodoList.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new RemoveTodoItemCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockTodoList.Verify(x => x.RemoveItem(1), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveTodoItemCommand(999);
        _mockTodoList.Setup(x => x.RemoveItem(999))
            .Throws(new TodoItemNotFoundException(999));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithItemOverThreshold_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveTodoItemCommand(1);
        _mockTodoList.Setup(x => x.RemoveItem(1))
            .Throws(new TodoItemCannotBeModifiedException(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}

public class RegisterProgressionCommandHandlerTests
{
    private readonly Mock<ITodoList> _mockTodoList;
    private readonly RegisterProgressionCommandHandler _handler;

    public RegisterProgressionCommandHandlerTests()
    {
        _mockTodoList = new Mock<ITodoList>();
        _handler = new RegisterProgressionCommandHandler(_mockTodoList.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var date = DateTime.Now;
        var command = new RegisterProgressionCommand(1, date, 25m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockTodoList.Verify(x => x.RegisterProgression(1, date, 25m), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterProgressionCommand(999, DateTime.Now, 25m);
        _mockTodoList.Setup(x => x.RegisterProgression(999, It.IsAny<DateTime>(), It.IsAny<decimal>()))
            .Throws(new TodoItemNotFoundException(999));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidProgression_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterProgressionCommand(1, DateTime.Now, 150m);
        _mockTodoList.Setup(x => x.RegisterProgression(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>()))
            .Throws(new InvalidProgressionException("Invalid"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
