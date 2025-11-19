// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using AutoMapper;
using FluentAssertions;
using TodoListManager.Application.DTOs;
using TodoListManager.Application.Mappings;
using TodoListManager.Domain.Entities;

namespace TodoListManager.Application.Tests.Mappings;

/// <summary>
/// Integration tests for AutoMapper profiles.
/// </summary>
public class TodoItemMappingProfileTests
{
    private readonly IMapper _mapper;

    public TodoItemMappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<TodoItemMappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<TodoItemMappingProfile>());

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_TodoItemToTodoItemDto_ShouldMapAllProperties()
    {
        var todoItem = new TodoItem(1, "Test Task", "Test Description", "Work");
        todoItem.AddProgression(DateTime.Now, 25m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 30m);

        var dto = _mapper.Map<TodoItemDto>(todoItem);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Title.Should().Be("Test Task");
        dto.Description.Should().Be("Test Description");
        dto.Category.Should().Be("Work");
        dto.TotalProgress.Should().Be(55m);
        dto.IsCompleted.Should().BeFalse();
        dto.Progressions.Should().HaveCount(2);
        dto.LastProgressionDate.Should().NotBeNull();
    }

    [Fact]
    public void Map_TodoItemWithNoProgressions_ShouldMapCorrectly()
    {
        var todoItem = new TodoItem(1, "Test Task", "Test Description", "Work");

        var dto = _mapper.Map<TodoItemDto>(todoItem);

        dto.Should().NotBeNull();
        dto.TotalProgress.Should().Be(0m);
        dto.IsCompleted.Should().BeFalse();
        dto.Progressions.Should().BeEmpty();
        dto.LastProgressionDate.Should().BeNull();
    }

    [Fact]
    public void Map_TodoItemWith100PercentProgress_ShouldMapAsCompleted()
    {
        var todoItem = new TodoItem(1, "Test Task", "Test Description", "Work");
        todoItem.AddProgression(DateTime.Now, 50m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 50m);

        var dto = _mapper.Map<TodoItemDto>(todoItem);

        dto.TotalProgress.Should().Be(100m);
        dto.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Map_ListOfTodoItems_ShouldMapAllItems()
    {
        var todoItems = new List<TodoItem>
        {
            new TodoItem(1, "Task 1", "Desc 1", "Work"),
            new TodoItem(2, "Task 2", "Desc 2", "Personal"),
            new TodoItem(3, "Task 3", "Desc 3", "Health")
        };

        var dtos = _mapper.Map<List<TodoItemDto>>(todoItems);

        dtos.Should().HaveCount(3);
        dtos[0].Id.Should().Be(1);
        dtos[1].Id.Should().Be(2);
        dtos[2].Id.Should().Be(3);
    }

    [Fact]
    public void Map_TodoItemDto_ShouldNotExposeInternalDomainLogic()
    {
        var todoItem = new TodoItem(1, "Test Task", "Test Description", "Work");

        var dto = _mapper.Map<TodoItemDto>(todoItem);

        dto.Should().BeAssignableTo<TodoItemDto>();
        
        // Get only custom methods, excluding record-generated methods (ToString, GetHashCode, Equals, etc.)
        var customMethods = dto.GetType().GetMethods()
            .Where(m => !m.IsSpecialName && m.DeclaringType == typeof(TodoItemDto))
            .Where(m => m.Name != "ToString" && 
                       m.Name != "GetHashCode" && 
                       m.Name != "Equals" &&
                       !m.Name.StartsWith("get_") && 
                       !m.Name.StartsWith("set_") &&
                       !m.Name.Contains("Clone") &&
                       !m.Name.Contains("Deconstruct") &&
                       !m.Name.Contains("PrintMembers"))
            .ToList();
            
        customMethods.Should().BeEmpty("DTOs should only contain data, not custom behavior");
    }

    [Fact]
    public void Map_ProgressionToProgressionDto_ShouldMapCorrectly()
    {
        var todoItem = new TodoItem(1, "Test", "Test", "Work");
        var date = new DateTime(2024, 1, 15);
        todoItem.AddProgression(date, 25m);

        var dto = _mapper.Map<TodoItemDto>(todoItem);
        var progressionDto = dto.Progressions.First();

        progressionDto.Date.Should().Be(date);
        progressionDto.Percent.Should().Be(25m);
    }
}
