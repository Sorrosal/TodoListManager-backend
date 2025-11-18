// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Specifications;

namespace TodoListManager.Domain.Tests.Specifications;

/// <summary>
/// Unit tests for CanModifyTodoItemSpecification following best practices.
/// </summary>
public class CanModifyTodoItemSpecificationTests
{
    private readonly CanModifyTodoItemSpecification _specification;

    public CanModifyTodoItemSpecificationTests()
    {
        _specification = new CanModifyTodoItemSpecification();
    }

    #region IsSatisfiedBy Tests

    [Fact]
    public void IsSatisfiedBy_WithNullItem_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _specification.IsSatisfiedBy(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsSatisfiedBy_WithNoProgress_ShouldReturnTrue()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");

        // Act
        var result = _specification.IsSatisfiedBy(todoItem);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithProgressUnder50Percent_ShouldReturnTrue()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 25m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 10m);

        // Act
        var result = _specification.IsSatisfiedBy(todoItem);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithExactly50Percent_ShouldReturnTrue()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 30m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 20m);

        // Act
        var result = _specification.IsSatisfiedBy(todoItem);

        // Assert
        result.Should().BeTrue();
        todoItem.GetTotalProgress().Should().Be(50m);
    }

    [Fact]
    public void IsSatisfiedBy_WithProgressOver50Percent_ShouldReturnFalse()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 51m);

        // Act
        var result = _specification.IsSatisfiedBy(todoItem);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(50.1)]
    [InlineData(75)]
    [InlineData(99.9)]
    [InlineData(100)]
    public void IsSatisfiedBy_WithVariousProgressValuesOver50_ShouldReturnFalse(decimal totalProgress)
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, totalProgress);

        // Act
        var result = _specification.IsSatisfiedBy(todoItem);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(25.5)]
    [InlineData(49.9)]
    [InlineData(50)]
    public void IsSatisfiedBy_WithVariousProgressValuesUpTo50_ShouldReturnTrue(decimal totalProgress)
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, totalProgress);

        // Act
        var result = _specification.IsSatisfiedBy(todoItem);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetReason Tests

    [Fact]
    public void GetReason_WithNullItem_ShouldReturnNullMessage()
    {
        // Act
        var reason = _specification.GetReason(null!);

        // Assert
        reason.Should().Be("Item is null");
    }

    [Fact]
    public void GetReason_WithValidItem_ShouldReturnEmptyString()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 30m);

        // Act
        var reason = _specification.GetReason(todoItem);

        // Assert
        reason.Should().BeEmpty();
    }

    [Fact]
    public void GetReason_WithItemOver50Percent_ShouldReturnDescriptiveMessage()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 60m);

        // Act
        var reason = _specification.GetReason(todoItem);

        // Assert
        reason.Should().Contain("cannot be modified");
        reason.Should().Contain("60");
        reason.Should().Contain("50");
    }

    [Fact]
    public void GetReason_WithExactly50Percent_ShouldReturnEmptyString()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 50m);

        // Act
        var reason = _specification.GetReason(todoItem);

        // Assert
        reason.Should().BeEmpty();
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void IsSatisfiedBy_BoundaryAt50Percent_ShouldBehaviorBeConsistent()
    {
        // Arrange - Just under 50%
        var itemUnder = new TodoItem(1, "Title", "Description", "Work");
        itemUnder.AddProgression(DateTime.Now, 49.99m);

        // Arrange - Exactly 50%
        var itemExact = new TodoItem(2, "Title", "Description", "Work");
        itemExact.AddProgression(DateTime.Now, 50m);

        // Arrange - Just over 50%
        var itemOver = new TodoItem(3, "Title", "Description", "Work");
        itemOver.AddProgression(DateTime.Now, 50.01m);

        // Act & Assert
        _specification.IsSatisfiedBy(itemUnder).Should().BeTrue();
        _specification.IsSatisfiedBy(itemExact).Should().BeTrue();
        _specification.IsSatisfiedBy(itemOver).Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Specification_ShouldWorkWithRealWorldScenario()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Complete Project", "Important project", "Work");

        // Initial state - can modify
        _specification.IsSatisfiedBy(todoItem).Should().BeTrue();
        _specification.GetReason(todoItem).Should().BeEmpty();

        // Add some progress - still can modify
        todoItem.AddProgression(new DateTime(2024, 1, 1), 20m);
        _specification.IsSatisfiedBy(todoItem).Should().BeTrue();

        // Add more progress - still can modify
        todoItem.AddProgression(new DateTime(2024, 1, 15), 30m);
        _specification.IsSatisfiedBy(todoItem).Should().BeTrue();

        // Add more progress - now cannot modify
        todoItem.AddProgression(new DateTime(2024, 1, 30), 5m);
        _specification.IsSatisfiedBy(todoItem).Should().BeFalse();
        _specification.GetReason(todoItem).Should().NotBeEmpty();
    }

    #endregion
}
