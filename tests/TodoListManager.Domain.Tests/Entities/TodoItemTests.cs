// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Tests.Entities;

/// <summary>
/// Unit tests for TodoItem entity following best practices.
/// </summary>
public class TodoItemTests
{
    #region Creation Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTodoItem()
    {
        // Arrange
        var id = 1;
        var title = "Test Task";
        var description = "Test Description";
        var category = "Work";

        // Act
        var todoItem = new TodoItem(id, title, description, category);

        // Assert
        todoItem.Should().NotBeNull();
        todoItem.Id.Should().Be(id);
        todoItem.Title.Should().Be(title);
        todoItem.Description.Should().Be(description);
        todoItem.Category.Should().Be(category);
        todoItem.Progressions.Should().BeEmpty();
        todoItem.IsCompleted.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Work")]
    [InlineData("Personal")]
    [InlineData("Health & Fitness")]
    public void Constructor_WithVariousCategories_ShouldSucceed(string category)
    {
        // Act
        var todoItem = new TodoItem(1, "Title", "Description", category);

        // Assert
        todoItem.Category.Should().Be(category);
    }

    #endregion

    #region UpdateDescription Tests

    [Fact]
    public void UpdateDescription_WithNewDescription_ShouldUpdateSuccessfully()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Original Description", "Work");
        var newDescription = "Updated Description";

        // Act
        todoItem.UpdateDescription(newDescription);

        // Assert
        todoItem.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateDescription_WithEmptyString_ShouldUpdate()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Original Description", "Work");

        // Act
        todoItem.UpdateDescription(string.Empty);

        // Assert
        todoItem.Description.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDescription_MultipleTimes_ShouldKeepLastValue()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Original", "Work");

        // Act
        todoItem.UpdateDescription("First Update");
        todoItem.UpdateDescription("Second Update");
        todoItem.UpdateDescription("Final Update");

        // Assert
        todoItem.Description.Should().Be("Final Update");
    }

    #endregion

    #region AddProgression Tests

    [Fact]
    public void AddProgression_WithValidProgression_ShouldAddToList()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        var date = DateTime.Now;
        var percent = 25m;

        // Act
        todoItem.AddProgression(date, percent);

        // Assert
        todoItem.Progressions.Should().HaveCount(1);
        todoItem.Progressions[0].Date.Should().Be(date);
        todoItem.Progressions[0].Percent.Should().Be(percent);
    }

    [Fact]
    public void AddProgression_MultipleProgressions_ShouldAddAllToList()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");

        // Act
        todoItem.AddProgression(DateTime.Now, 20m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 30m);
        todoItem.AddProgression(DateTime.Now.AddDays(2), 15m);

        // Assert
        todoItem.Progressions.Should().HaveCount(3);
    }

    [Fact]
    public void AddProgression_ShouldNotAllowDirectModificationOfProgressions()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 20m);

        // Act & Assert
        todoItem.Progressions.Should().BeAssignableTo<IReadOnlyList<Progression>>();
    }

    #endregion

    #region GetTotalProgress Tests

    [Fact]
    public void GetTotalProgress_WithNoProgressions_ShouldReturnZero()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");

        // Act
        var totalProgress = todoItem.GetTotalProgress();

        // Assert
        totalProgress.Should().Be(0m);
    }

    [Fact]
    public void GetTotalProgress_WithSingleProgression_ShouldReturnThatValue()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 35m);

        // Act
        var totalProgress = todoItem.GetTotalProgress();

        // Assert
        totalProgress.Should().Be(35m);
    }

    [Fact]
    public void GetTotalProgress_WithMultipleProgressions_ShouldReturnSum()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 20m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 30m);
        todoItem.AddProgression(DateTime.Now.AddDays(2), 15m);

        // Act
        var totalProgress = todoItem.GetTotalProgress();

        // Assert
        totalProgress.Should().Be(65m);
    }

    [Theory]
    [InlineData(25, 25, 25, 25)]
    [InlineData(10, 20, 30, 40)]
    [InlineData(50, 30, 15, 5)]
    public void GetTotalProgress_WithVariousCombinations_ShouldCalculateCorrectly(
        decimal p1, decimal p2, decimal p3, decimal p4)
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, p1);
        todoItem.AddProgression(DateTime.Now.AddDays(1), p2);
        todoItem.AddProgression(DateTime.Now.AddDays(2), p3);
        todoItem.AddProgression(DateTime.Now.AddDays(3), p4);

        // Act
        var totalProgress = todoItem.GetTotalProgress();

        // Assert
        totalProgress.Should().Be(p1 + p2 + p3 + p4);
    }

    #endregion

    #region IsCompleted Tests

    [Fact]
    public void IsCompleted_WithNoProgress_ShouldBeFalse()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");

        // Assert
        todoItem.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void IsCompleted_WithLessThan100Percent_ShouldBeFalse()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 50m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 30m);

        // Assert
        todoItem.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void IsCompleted_WithExactly100Percent_ShouldBeTrue()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 50m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 50m);

        // Assert
        todoItem.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void IsCompleted_WithMoreThan100Percent_ShouldBeTrue()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 60m);
        todoItem.AddProgression(DateTime.Now.AddDays(1), 50m);

        // Assert
        todoItem.IsCompleted.Should().BeTrue();
        todoItem.GetTotalProgress().Should().Be(110m);
    }

    #endregion

    #region GetLastProgressionDate Tests

    [Fact]
    public void GetLastProgressionDate_WithNoProgressions_ShouldReturnNull()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");

        // Act
        var lastDate = todoItem.GetLastProgressionDate();

        // Assert
        lastDate.Should().BeNull();
    }

    [Fact]
    public void GetLastProgressionDate_WithSingleProgression_ShouldReturnThatDate()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        var date = new DateTime(2024, 1, 15);
        todoItem.AddProgression(date, 25m);

        // Act
        var lastDate = todoItem.GetLastProgressionDate();

        // Assert
        lastDate.Should().Be(date);
    }

    [Fact]
    public void GetLastProgressionDate_WithMultipleProgressions_ShouldReturnLatestDate()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        var date1 = new DateTime(2024, 1, 15);
        var date2 = new DateTime(2024, 1, 20);
        var date3 = new DateTime(2024, 1, 18);

        todoItem.AddProgression(date1, 20m);
        todoItem.AddProgression(date2, 30m);
        todoItem.AddProgression(date3, 15m);

        // Act
        var lastDate = todoItem.GetLastProgressionDate();

        // Assert
        lastDate.Should().Be(date2);
    }

    [Fact]
    public void GetLastProgressionDate_WithProgressionsInRandomOrder_ShouldReturnLatest()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        var latestDate = new DateTime(2024, 12, 31);

        todoItem.AddProgression(new DateTime(2024, 3, 15), 10m);
        todoItem.AddProgression(latestDate, 20m);
        todoItem.AddProgression(new DateTime(2024, 6, 10), 15m);
        todoItem.AddProgression(new DateTime(2024, 1, 1), 5m);

        // Act
        var lastDate = todoItem.GetLastProgressionDate();

        // Assert
        lastDate.Should().Be(latestDate);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void TodoItem_FullWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Complete Project", "Finish the project", "Work");

        // Act & Assert - Initial state
        todoItem.GetTotalProgress().Should().Be(0m);
        todoItem.IsCompleted.Should().BeFalse();
        todoItem.GetLastProgressionDate().Should().BeNull();

        // Act & Assert - Add first progression
        todoItem.AddProgression(new DateTime(2024, 1, 1), 25m);
        todoItem.GetTotalProgress().Should().Be(25m);
        todoItem.IsCompleted.Should().BeFalse();

        // Act & Assert - Update description
        todoItem.UpdateDescription("Finish the project by month end");
        todoItem.Description.Should().Be("Finish the project by month end");

        // Act & Assert - Add more progressions
        todoItem.AddProgression(new DateTime(2024, 1, 15), 30m);
        todoItem.AddProgression(new DateTime(2024, 1, 30), 45m);
        todoItem.GetTotalProgress().Should().Be(100m);
        todoItem.IsCompleted.Should().BeTrue();
        todoItem.GetLastProgressionDate().Should().Be(new DateTime(2024, 1, 30));
    }

    [Fact]
    public void Progressions_ShouldBeImmutableFromOutside()
    {
        // Arrange
        var todoItem = new TodoItem(1, "Title", "Description", "Work");
        todoItem.AddProgression(DateTime.Now, 25m);

        // Act
        var progressions = todoItem.Progressions;

        // Assert
        progressions.Should().BeAssignableTo<IReadOnlyList<Progression>>();
        var act = () => ((List<Progression>)progressions).Add(new Progression(DateTime.Now, 10m));
        act.Should().Throw<InvalidCastException>();
    }

    #endregion
}
