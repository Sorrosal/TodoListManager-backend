// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Application.DTOs;
using TodoListManager.Application.Handlers;
using TodoListManager.Application.Queries;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Repositories;
using AutoMapper;

namespace TodoListManager.Application.Tests.Handlers;

/// <summary>
/// Unit tests for GetAllTodoItemsQueryHandler following AAA pattern.
/// </summary>
public class GetAllTodoItemsQueryHandlerTests
{
    private readonly Mock<ITodoListRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetAllTodoItemsQueryHandler _handler;

    public GetAllTodoItemsQueryHandlerTests()
    {
        _mockRepository = new Mock<ITodoListRepository>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetAllTodoItemsQueryHandler(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyResult()
    {
        var emptyList = new List<TodoItem>();
        var emptyDtoList = new List<TodoItemDto>();
        
        _mockRepository.Setup(r => r.GetAllDomainItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);
        
        _mockMapper.Setup(m => m.Map<List<TodoItemDto>>(emptyList))
            .Returns(emptyDtoList);
        
        var query = new GetAllTodoItemsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithItems_ShouldReturnAllItemsAsDTOs()
    {
        var domainItems = new List<TodoItem>
        {
            new TodoItem(1, "Task 1", "Desc 1", "Work"),
            new TodoItem(2, "Task 2", "Desc 2", "Personal")
        };
        
        var dtoItems = new List<TodoItemDto>
        {
            new TodoItemDto { Id = 1, Title = "Task 1", Description = "Desc 1", Category = "Work" },
            new TodoItemDto { Id = 2, Title = "Task 2", Description = "Desc 2", Category = "Personal" }
        };
        
        _mockRepository.Setup(r => r.GetAllDomainItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainItems);
        
        _mockMapper.Setup(m => m.Map<List<TodoItemDto>>(domainItems))
            .Returns(dtoItems);
        
        var query = new GetAllTodoItemsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items[0].Id.Should().Be(1);
        result.Value.Items[1].Id.Should().Be(2);
        _mockMapper.Verify(m => m.Map<List<TodoItemDto>>(domainItems), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnItemsSortedById()
    {
        var domainItems = new List<TodoItem>
        {
            new TodoItem(3, "Task 3", "Desc 3", "Work"),
            new TodoItem(1, "Task 1", "Desc 1", "Work"),
            new TodoItem(2, "Task 2", "Desc 2", "Work")
        };
        
        var dtoItems = new List<TodoItemDto>
        {
            new TodoItemDto { Id = 3, Title = "Task 3", Description = "Desc 3", Category = "Work" },
            new TodoItemDto { Id = 1, Title = "Task 1", Description = "Desc 1", Category = "Work" },
            new TodoItemDto { Id = 2, Title = "Task 2", Description = "Desc 2", Category = "Work" }
        };
        
        _mockRepository.Setup(r => r.GetAllDomainItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainItems);
        
        _mockMapper.Setup(m => m.Map<List<TodoItemDto>>(domainItems))
            .Returns(dtoItems);
        
        var query = new GetAllTodoItemsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
    }
    
    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        Action act = () => new GetAllTodoItemsQueryHandler(null!, _mockMapper.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("repository");
    }
    
    [Fact]
    public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
    {
        Action act = () => new GetAllTodoItemsQueryHandler(_mockRepository.Object, null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("mapper");
    }
}
