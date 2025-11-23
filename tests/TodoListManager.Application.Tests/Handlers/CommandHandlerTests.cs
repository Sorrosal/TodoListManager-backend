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

public class UpdateTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoListRepository> _mockRepository;
    private readonly Mock<TodoListManager.Domain.Common.IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateTodoItemCommandHandler _handler;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;

    public UpdateTodoItemCommandHandlerTests()
    {
        _mockRepository = new Mock<ITodoListRepository>();
        _mockUnitOfWork = new Mock<TodoListManager.Domain.Common.IUnitOfWork>();
        _handler = new UpdateTodoItemCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
        _canModifySpecification = new CanModifyTodoItemSpecification();
        _validProgressionSpecification = new ValidProgressionSpecification();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(1, "Updated Description");

        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);

        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        aggregate.AddItem(1, "Title", "Desc", "Work");
        var item = aggregate.GetAllItems().First(i => i.Id == 1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = aggregate.GetAllItems().First(i => i.Id == 1);
        updated.Description.Should().Be("Updated Description");
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(999, "Description");
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WithItemOverThreshold_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(1, "Description");

        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);

        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        aggregate.AddItem(1, "Title", "Desc", "Work");
        // add progression to exceed 50%
        aggregate.RegisterProgression(1, DateTime.UtcNow.AddDays(-1), 60m);
        var item = aggregate.GetAllItems().First(i => i.Id == 1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);

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

        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);

        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        aggregate.AddItem(1, "Title", "Desc", "Work");
        var item = aggregate.GetAllItems().First(i => i.Id == 1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Throws(new DomainException("Domain error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Domain error");
    }
}

public class RemoveTodoItemCommandHandlerTests
{
    private readonly Mock<ITodoListRepository> _mockRepository;
    private readonly Mock<TodoListManager.Domain.Common.IUnitOfWork> _mockUnitOfWork;
    private readonly RemoveTodoItemCommandHandler _handler;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;

    public RemoveTodoItemCommandHandlerTests()
    {
        _mockRepository = new Mock<ITodoListRepository>();
        _mockUnitOfWork = new Mock<TodoListManager.Domain.Common.IUnitOfWork>();
        _handler = new RemoveTodoItemCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
        _canModifySpecification = new CanModifyTodoItemSpecification();
        _validProgressionSpecification = new ValidProgressionSpecification();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new RemoveTodoItemCommand(1);

        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        aggregate.AddItem(1, "Title", "Desc", "Work");
        var item = aggregate.GetAllItems().First(i => i.Id == 1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);
        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveTodoItemCommand(999);
        
        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((TodoItem?)null);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);

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

        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        aggregate.AddItem(1, "Title", "Desc", "Work");
        aggregate.RegisterProgression(1, DateTime.UtcNow.AddDays(-1), 60m);
        var item = aggregate.GetAllItems().First(i => i.Id == 1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}

public class RegisterProgressionCommandHandlerTests
{
    private readonly Mock<ITodoListRepository> _mockRepository;
    private readonly Mock<TodoListManager.Domain.Common.IUnitOfWork> _mockUnitOfWork;
    private readonly RegisterProgressionCommandHandler _handler;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;

    public RegisterProgressionCommandHandlerTests()
    {
        _mockRepository = new Mock<ITodoListRepository>();
        _mockUnitOfWork = new Mock<TodoListManager.Domain.Common.IUnitOfWork>();
        _handler = new RegisterProgressionCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
        _canModifySpecification = new CanModifyTodoItemSpecification();
        _validProgressionSpecification = new ValidProgressionSpecification();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var date = DateTime.Now;
        var command = new RegisterProgressionCommand(1, date, 25m);

        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        aggregate.AddItem(1, "Title", "Desc", "Work");
        var item = aggregate.GetAllItems().First(i => i.Id == 1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = aggregate.GetAllItems().First(i => i.Id == 1);
        updated.Progressions.Should().HaveCount(1);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterProgressionCommand(999, DateTime.Now, 25m);
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((TodoItem?)null);

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

        var categoryValidator = new Mock<ICategoryValidator>();
        categoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        var aggregate = new TodoList(categoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
        aggregate.AddItem(1, "Title", "Desc", "Work");
        var item = aggregate.GetAllItems().First(i => i.Id == 1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _mockRepository.Setup(r => r.GetAggregateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
