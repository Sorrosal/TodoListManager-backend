// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Infrastructure.Services;

namespace TodoListManager.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for CategoryValidator service.
/// </summary>
public class CategoryValidatorTests
{
    private readonly CategoryValidator _validator;

    public CategoryValidatorTests()
    {
        _validator = new CategoryValidator();
    }

    #region IsValidCategory Tests

    [Fact]
    public void IsValidCategory_WithValidCategory_ShouldReturnTrue()
    {
        // Act
        var result = _validator.IsValidCategory("Work");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidCategory_ShouldBeCaseInsensitive()
    {
        // Act & Assert
        _validator.IsValidCategory("work").Should().BeTrue();
        _validator.IsValidCategory("WORK").Should().BeTrue();
        _validator.IsValidCategory("WoRk").Should().BeTrue();
        _validator.IsValidCategory("personal").Should().BeTrue();
        _validator.IsValidCategory("PERSONAL").Should().BeTrue();
    }

    [Fact]
    public void IsValidCategory_WithInvalidCategory_ShouldReturnFalse()
    {
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

    [Theory]
    [InlineData("Work")]
    [InlineData("Personal")]
    [InlineData("Education")]
    [InlineData("Health")]
    [InlineData("Finance")]
    [InlineData("Other")]
    public void IsValidCategory_WithAllValidCategories_ShouldReturnTrue(string category)
    {
        // Act
        var result = _validator.IsValidCategory(category);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetValidCategories Tests

    [Fact]
    public void GetValidCategories_ShouldReturnAllCategories()
    {
        // Arrange
        var expectedCategories = new[] { "Work", "Personal", "Education", "Health", "Finance", "Other" };

        // Act
        var result = _validator.GetValidCategories();

        // Assert
        result.Should().BeEquivalentTo(expectedCategories);
    }

    [Fact]
    public void GetValidCategories_ShouldReturnReadOnlyCollection()
    {
        // Act
        var result = _validator.GetValidCategories();

        // Assert
        result.Should().BeAssignableTo<IReadOnlyCollection<string>>();
    }

    [Fact]
    public void GetValidCategories_ShouldReturnSameInstanceEachTime()
    {
        // Act
        var result1 = _validator.GetValidCategories();
        var result2 = _validator.GetValidCategories();

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    #endregion
}
