// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Services;
using TodoListManager.Domain.Specifications;

namespace TodoListManager.Domain.Tests.Aggregates;

/// <summary>
/// Unit tests for TodoList aggregate following best practices.
/// Tests business rules and invariants enforcement.
/// </summary>
public class TodoListTests
{
    private readonly Mock<ICategoryValidator> _mockCategoryValidator;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;
    private readonly TodoList _todoList;

    public TodoListTests()
    {
        _mockCategoryValidator = new Mock<ICategoryValidator>();
        _canModifySpecification = new CanModifyTodoItemSpecification();
        _validProgressionSpecification = new ValidProgressionSpecification();
        _todoList = new TodoList(_mockCategoryValidator.Object, _canModifySpecification, _validProgressionSpecification);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidCategoryValidator_ShouldCreateInstance()
    {
        // Arrange
        var categoryValidator = new Mock<ICategoryValidator>();
        var canModifySpec = new CanModifyTodoItemSpecification();
        var validProgressionSpec = new ValidProgressionSpecification();

        // Act
        var todoList = new TodoList(categoryValidator.Object, canModifySpec, validProgressionSpec);

        // Assert
        todoList.Should().NotBeNull();
        todoList.GetAllItems().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullCategoryValidator_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new TodoList(null!, _canModifySpecification, _validProgressionSpecification);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("categoryValidator");
    }

    #endregion

    #region AddItem Tests

    [Fact]
    public void AddItem_WithValidCategory_ShouldAddItemSuccessfully()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory("Work")).Returns(true);

        // Act
        _todoList.AddItem(1, "Task 1", "Description 1", "Work");

        // Assert
        var items = _todoList.GetAllItems();
        items.Should().HaveCount(1);
        items[0].Id.Should().Be(1);
        items[0].Title.Should().Be("Task 1");
        items[0].Description.Should().Be("Description 1");
        items[0].Category.Should().Be("Work");
        _mockCategoryValidator.Verify(x => x.IsValidCategory("Work"), Times.Once);
    }

    [Fact]
    public void AddItem_WithInvalidCategory_ShouldThrowInvalidCategoryException()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory("InvalidCategory")).Returns(false);

        // Act
        Action act = () => _todoList.AddItem(1, "Task", "Description", "InvalidCategory");

        // Assert
        act.Should().Throw<InvalidCategoryException>()
            .WithMessage("*InvalidCategory*");
        _todoList.GetAllItems().Should().BeEmpty();
    }

    [Fact]
    public void AddItem_MultipleItems_ShouldAddAll()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);

        // Act
        _todoList.AddItem(1, "Task 1", "Description 1", "Work");
        _todoList.AddItem(2, "Task 2", "Description 2", "Personal");
        _todoList.AddItem(3, "Task 3", "Description 3", "Health");

        // Assert
        _todoList.GetAllItems().Should().HaveCount(3);
    }

    [Fact]
    public void AddItem_WithDuplicateId_ShouldReplaceExistingItem()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Original Task", "Original Description", "Work");

        // Act
        _todoList.AddItem(1, "Updated Task", "Updated Description", "Personal");

        // Assert
        var items = _todoList.GetAllItems();
        items.Should().HaveCount(1);
        items[0].Title.Should().Be("Updated Task");
        items[0].Description.Should().Be("Updated Description");
    }

    #endregion

    #region UpdateItem Tests

    [Fact]
    public void UpdateItem_WithValidIdAndLowProgress_ShouldUpdateDescription()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Original Description", "Work");

        // Act
        _todoList.UpdateItem(1, "Updated Description");

        // Assert
        var items = _todoList.GetAllItems();
        items[0].Description.Should().Be("Updated Description");
    }

    [Fact]
    public void UpdateItem_WithNonExistentId_ShouldThrowTodoItemNotFoundException()
    {
        // Act
        Action act = () => _todoList.UpdateItem(999, "New Description");

        // Assert
        act.Should().Throw<TodoItemNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public void UpdateItem_WithProgressAbove50Percent_ShouldThrowTodoItemCannotBeModifiedException()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        _todoList.RegisterProgression(1, DateTime.Now, 51m);

        // Act
        Action act = () => _todoList.UpdateItem(1, "New Description");

        // Assert
        act.Should().Throw<TodoItemCannotBeModifiedException>()
            .WithMessage("*1*");
    }

    [Fact]
    public void UpdateItem_WithExactly50Percent_ShouldThrowTodoItemCannotBeModifiedException()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        _todoList.RegisterProgression(1, DateTime.Now, 50m);
        _todoList.RegisterProgression(1, DateTime.Now.AddDays(1), 0.1m);

        // Act
        Action act = () => _todoList.UpdateItem(1, "New Description");

        // Assert
        act.Should().Throw<TodoItemCannotBeModifiedException>();
    }

    [Fact]
    public void UpdateItem_WithProgressExactly50Percent_ShouldSucceed()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        _todoList.RegisterProgression(1, DateTime.Now, 50m);

        // Act
        _todoList.UpdateItem(1, "New Description");

        // Assert
        var items = _todoList.GetAllItems();
        items[0].Description.Should().Be("New Description");
    }

    #endregion

    #region RemoveItem Tests

    [Fact]
    public void RemoveItem_WithValidIdAndLowProgress_ShouldRemoveItem()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");

        // Act
        _todoList.RemoveItem(1);

        // Assert
        _todoList.GetAllItems().Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WithNonExistentId_ShouldThrowTodoItemNotFoundException()
    {
        // Act
        Action act = () => _todoList.RemoveItem(999);

        // Assert
        act.Should().Throw<TodoItemNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public void RemoveItem_WithProgressAbove50Percent_ShouldThrowTodoItemCannotBeModifiedException()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        _todoList.RegisterProgression(1, DateTime.Now, 51m);

        // Act
        Action act = () => _todoList.RemoveItem(1);

        // Assert
        act.Should().Throw<TodoItemCannotBeModifiedException>()
            .WithMessage("*1*");
    }

    [Fact]
    public void RemoveItem_WithMultipleItems_ShouldRemoveOnlySpecified()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task 1", "Description 1", "Work");
        _todoList.AddItem(2, "Task 2", "Description 2", "Work");
        _todoList.AddItem(3, "Task 3", "Description 3", "Work");

        // Act
        _todoList.RemoveItem(2);

        // Assert
        var items = _todoList.GetAllItems();
        items.Should().HaveCount(2);
        items.Should().NotContain(item => item.Id == 2);
    }

    #endregion

    #region RegisterProgression Tests

    [Fact]
    public void RegisterProgression_WithValidParameters_ShouldAddProgression()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        var date = DateTime.Now;

        // Act
        _todoList.RegisterProgression(1, date, 25m);

        // Assert
        var item = _todoList.GetAllItems()[0];
        item.Progressions.Should().HaveCount(1);
        item.GetTotalProgress().Should().Be(25m);
    }

    [Fact]
    public void RegisterProgression_WithNonExistentId_ShouldThrowTodoItemNotFoundException()
    {
        // Act
        Action act = () => _todoList.RegisterProgression(999, DateTime.Now, 25m);

        // Assert
        act.Should().Throw<TodoItemNotFoundException>()
            .WithMessage("*999*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(101)]
    public void RegisterProgression_WithInvalidPercent_ShouldThrowInvalidProgressionException(decimal percent)
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");

        // Act
        Action act = () => _todoList.RegisterProgression(1, DateTime.Now, percent);

        // Assert
        act.Should().Throw<InvalidProgressionException>()
            .WithMessage("*Percent must be greater than 0 and less than 100*");
    }

    [Fact]
    public void RegisterProgression_WithDateEarlierThanExisting_ShouldThrowInvalidProgressionException()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        var laterDate = new DateTime(2024, 1, 20);
        var earlierDate = new DateTime(2024, 1, 15);
        
        _todoList.RegisterProgression(1, laterDate, 25m);

        // Act
        Action act = () => _todoList.RegisterProgression(1, earlierDate, 20m);

        // Assert
        act.Should().Throw<InvalidProgressionException>()
            .WithMessage("*must be greater than all existing progression dates*");
    }

    [Fact]
    public void RegisterProgression_WithSameDateAsExisting_ShouldThrowInvalidProgressionException()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        var date = new DateTime(2024, 1, 15, 10, 0, 0);
        
        _todoList.RegisterProgression(1, date, 25m);

        // Act
        Action act = () => _todoList.RegisterProgression(1, date, 20m);

        // Assert
        act.Should().Throw<InvalidProgressionException>();
    }

    [Fact]
    public void RegisterProgression_ThatExceeds100Percent_ShouldThrowInvalidProgressionException()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        _todoList.RegisterProgression(1, DateTime.Now, 60m);

        // Act
        Action act = () => _todoList.RegisterProgression(1, DateTime.Now.AddDays(1), 50m);

        // Assert
        act.Should().Throw<InvalidProgressionException>()
            .WithMessage("*would exceed 100% total progress*");
    }

    [Fact]
    public void RegisterProgression_ThatReachesExactly100Percent_ShouldSucceed()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");
        _todoList.RegisterProgression(1, DateTime.Now, 40m);

        // Act
        _todoList.RegisterProgression(1, DateTime.Now.AddDays(1), 60m);

        // Assert
        var item = _todoList.GetAllItems()[0];
        item.GetTotalProgress().Should().Be(100m);
        item.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void RegisterProgression_MultipleProgressions_ShouldMaintainOrder()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");

        // Act
        _todoList.RegisterProgression(1, new DateTime(2024, 1, 1), 20m);
        _todoList.RegisterProgression(1, new DateTime(2024, 1, 5), 15m);
        _todoList.RegisterProgression(1, new DateTime(2024, 1, 10), 25m);

        // Assert
        var item = _todoList.GetAllItems()[0];
        item.Progressions.Should().HaveCount(3);
        item.GetTotalProgress().Should().Be(60m);
    }

    #endregion

    #region GetAllItems Tests

    [Fact]
    public void GetAllItems_WithNoItems_ShouldReturnEmptyList()
    {
        // Act
        var items = _todoList.GetAllItems();

        // Assert
        items.Should().BeEmpty();
        items.Should().BeAssignableTo<IReadOnlyList<TodoListManager.Domain.Entities.TodoItem>>();
    }

    [Fact]
    public void GetAllItems_WithMultipleItems_ShouldReturnSortedById()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(3, "Task 3", "Description 3", "Work");
        _todoList.AddItem(1, "Task 1", "Description 1", "Work");
        _todoList.AddItem(2, "Task 2", "Description 2", "Work");

        // Act
        var items = _todoList.GetAllItems();

        // Assert
        items.Should().HaveCount(3);
        items[0].Id.Should().Be(1);
        items[1].Id.Should().Be(2);
        items[2].Id.Should().Be(3);
    }

    [Fact]
    public void GetAllItems_ShouldReturnReadOnlyList()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");

        // Act
        var items = _todoList.GetAllItems();

        // Assert
        items.Should().BeAssignableTo<IReadOnlyList<TodoListManager.Domain.Entities.TodoItem>>();
    }

    #endregion

    #region Integration and Complex Scenarios

    [Fact]
    public void TodoList_CompleteWorkflow_ShouldMaintainBusinessRules()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);

        // Act & Assert - Add items
        _todoList.AddItem(1, "Task 1", "Description 1", "Work");
        _todoList.AddItem(2, "Task 2", "Description 2", "Personal");
        _todoList.GetAllItems().Should().HaveCount(2);

        // Act & Assert - Update item with low progress
        _todoList.UpdateItem(1, "Updated Description");
        _todoList.GetAllItems()[0].Description.Should().Be("Updated Description");

        // Act & Assert - Add progression
        _todoList.RegisterProgression(1, DateTime.Now, 30m);
        _todoList.GetAllItems()[0].GetTotalProgress().Should().Be(30m);

        // Act & Assert - Update still works (under 50%)
        _todoList.UpdateItem(1, "Another Update");
        _todoList.GetAllItems()[0].Description.Should().Be("Another Update");

        // Act & Assert - Add more progression (over 50%)
        _todoList.RegisterProgression(1, DateTime.Now.AddDays(1), 25m);
        _todoList.GetAllItems()[0].GetTotalProgress().Should().Be(55m);

        // Act & Assert - Update should fail (over 50%)
        Action actUpdate = () => _todoList.UpdateItem(1, "Should Fail");
        actUpdate.Should().Throw<TodoItemCannotBeModifiedException>();

        // Act & Assert - Remove should fail (over 50%)
        Action actRemove = () => _todoList.RemoveItem(1);
        actRemove.Should().Throw<TodoItemCannotBeModifiedException>();

        // Act & Assert - Can still remove item with low progress
        _todoList.RemoveItem(2);
        _todoList.GetAllItems().Should().HaveCount(1);
    }

    [Fact]
    public void TodoList_WithInvalidCategory_ShouldNotAddItemAndMaintainState()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory("ValidCategory")).Returns(true);
        _mockCategoryValidator.Setup(x => x.IsValidCategory("InvalidCategory")).Returns(false);
        _todoList.AddItem(1, "Task 1", "Description 1", "ValidCategory");

        // Act
        Action act = () => _todoList.AddItem(2, "Task 2", "Description 2", "InvalidCategory");

        // Assert
        act.Should().Throw<InvalidCategoryException>();
        _todoList.GetAllItems().Should().HaveCount(1);
    }

    [Fact]
    public void TodoList_ProgressionBoundaryConditions_ShouldEnforceCorrectly()
    {
        // Arrange
        _mockCategoryValidator.Setup(x => x.IsValidCategory(It.IsAny<string>())).Returns(true);
        _todoList.AddItem(1, "Task", "Description", "Work");

        // Act & Assert - Valid progressions
        _todoList.RegisterProgression(1, new DateTime(2024, 1, 1), 0.1m);
        _todoList.RegisterProgression(1, new DateTime(2024, 1, 2), 49.9m);
        _todoList.RegisterProgression(1, new DateTime(2024, 1, 3), 49.9m);

        // Assert - Just under 100%
        var item = _todoList.GetAllItems()[0];
        item.GetTotalProgress().Should().Be(99.9m);
        item.IsCompleted.Should().BeFalse();

        // Act & Assert - Attempt to exceed
        Action act = () => _todoList.RegisterProgression(1, new DateTime(2024, 1, 4), 0.2m);
        act.Should().Throw<InvalidProgressionException>();
    }

    #endregion
}
