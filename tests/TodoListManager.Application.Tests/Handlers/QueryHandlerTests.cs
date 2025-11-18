// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Application.Handlers;
using TodoListManager.Application.Queries;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Services;

namespace TodoListManager.Application.Tests.Handlers;

/// <summary>
/// Unit tests for GetAllTodoItemsQueryHandler.
/// </summary>
public class GetAllTodoItemsQueryHandlerTests
{
    private readonly Mock<ICategoryValidator> _mockCategoryValidator;
    private readonly TodoList _todoList;
    private readonly GetAllTodoItemsQueryHandler _handler;

    public GetAllTodoItemsQueryHandlerTests()
    {
        _mockCategoryValidator = new Mock<ICategoryValidator>();
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList = new TodoList(_mockCategoryValidator.Object);
        _handler = new GetAllTodoItemsQueryHandler(_todoList);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new GetAllTodoItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithItems_ShouldReturnAllItems()
    {
        // Arrange
        _todoList.AddItem(1, "Task 1", "Desc 1", "Work");
        _todoList.AddItem(2, "Task 2", "Desc 2", "Personal");
        var query = new GetAllTodoItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items[0].Id.Should().Be(1);
        result.Value.Items[1].Id.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnItemsSortedById()
    {
        // Arrange
        _todoList.AddItem(3, "Task 3", "Desc 3", "Work");
        _todoList.AddItem(1, "Task 1", "Desc 1", "Work");
        _todoList.AddItem(2, "Task 2", "Desc 2", "Work");
        var query = new GetAllTodoItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Items.Should().HaveCount(3);
        result.Value.Items.Select(x => x.Id).Should().BeInAscendingOrder();
    }
}
