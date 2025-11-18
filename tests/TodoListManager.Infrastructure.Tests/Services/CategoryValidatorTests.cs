// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Domain.Repositories;
using TodoListManager.Infrastructure.Services;

namespace TodoListManager.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for CategoryValidator service.
/// </summary>
public class CategoryValidatorTests
{
    private readonly Mock<ITodoListRepository> _mockRepository;
    private readonly CategoryValidator _validator;

    public CategoryValidatorTests()
    {
        _mockRepository = new Mock<ITodoListRepository>();
        _validator = new CategoryValidator(_mockRepository.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new CategoryValidator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region IsValidCategory Tests

    [Fact]
    public void IsValidCategory_WithValidCategory_ShouldReturnTrue()
    {
        // Arrange
        var validCategories = new List<string> { "Work", "Personal", "Health" };
        _mockRepository.Setup(x => x.GetAllCategories()).Returns(validCategories);

        // Act
        var result = _validator.IsValidCategory("Work");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidCategory_ShouldBeCaseInsensitive()
    {
        // Arrange
        var validCategories = new List<string> { "Work" };
        _mockRepository.Setup(x => x.GetAllCategories()).Returns(validCategories);

        // Act & Assert
        _validator.IsValidCategory("work").Should().BeTrue();
        _validator.IsValidCategory("WORK").Should().BeTrue();
        _validator.IsValidCategory("WoRk").Should().BeTrue();
    }

    [Fact]
    public void IsValidCategory_WithInvalidCategory_ShouldReturnFalse()
    {
        // Arrange
        var validCategories = new List<string> { "Work", "Personal" };
        _mockRepository.Setup(x => x.GetAllCategories()).Returns(validCategories);

        // Act
        var result = _validator.IsValidCategory("InvalidCategory");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidCategory_WithNullOrEmpty_ShouldReturnFalse(string category)
    {
        // Act
        var result = _validator.IsValidCategory(category);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetValidCategories Tests

    [Fact]
    public void GetValidCategories_ShouldReturnAllCategories()
    {
        // Arrange
        var expectedCategories = new List<string> { "Work", "Personal", "Health" };
        _mockRepository.Setup(x => x.GetAllCategories()).Returns(expectedCategories);

        // Act
        var result = _validator.GetValidCategories();

        // Assert
        result.Should().BeEquivalentTo(expectedCategories);
    }

    [Fact]
    public void GetValidCategories_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAllCategories()).Returns(new List<string> { "Work" });

        // Act
        var result = _validator.GetValidCategories();

        // Assert
        result.Should().BeAssignableTo<IReadOnlyCollection<string>>();
    }

    #endregion
}
