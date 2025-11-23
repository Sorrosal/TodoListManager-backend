// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.Specifications;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Tests.Specifications;

/// <summary>
/// Unit tests for ValidProgressionSpecification following best practices.
/// </summary>
public class ValidProgressionSpecificationTests
{
    private readonly ValidProgressionSpecification _specification;

    public ValidProgressionSpecificationTests()
    {
        _specification = new ValidProgressionSpecification();
    }

    #region IsSatisfiedBy Tests

    [Fact]
    public void IsSatisfiedBy_WithNullProgression_ShouldReturnFalse()
    {
        // Act
        var result = _specification.IsSatisfiedBy(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(99)]
    [InlineData(99.9)]
    public void IsSatisfiedBy_WithValidPercent_ShouldReturnTrue(decimal percent)
    {
        // Arrange
        var progression = new Progression(DateTime.Now, percent);

        // Act
        var result = _specification.IsSatisfiedBy(progression);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithZeroPercent_ShouldReturnFalse()
    {
        // Arrange - Use IsValidPercent method instead of creating invalid Progression
        var percent = 0m;

        // Act
        var result = _specification.IsValidPercent(percent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithNegativePercent_ShouldReturnFalse()
    {
        // Arrange - Use IsValidPercent method instead of creating invalid Progression
        var percent = -10m;

        // Act
        var result = _specification.IsValidPercent(percent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithExactly100Percent_ShouldReturnFalse()
    {
        // Arrange - Use IsValidPercent method instead
        var percent = 100m;

        // Act
        var result = _specification.IsValidPercent(percent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithPercentOver100_ShouldReturnFalse()
    {
        // Arrange - Use IsValidPercent method instead
        var percent = 150m;

        // Act
        var result = _specification.IsValidPercent(percent);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(-50)]
    [InlineData(-1)]
    [InlineData(-0.1)]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(100.1)]
    [InlineData(200)]
    public void IsSatisfiedBy_WithInvalidPercents_ShouldReturnFalse(decimal percent)
    {
        // Act - Use IsValidPercent method instead of creating invalid Progression
        var result = _specification.IsValidPercent(percent);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetReason Tests

    [Fact]
    public void GetReason_WithNullProgression_ShouldReturnNullMessage()
    {
        // Act
        var reason = _specification.GetReason(null!);

        // Assert
        reason.Should().Be("Progression is null");
    }

    [Fact]
    public void GetReason_WithValidProgression_ShouldReturnEmptyString()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, 50m);

        // Act
        var reason = _specification.GetReason(progression);

        // Assert
        reason.Should().BeEmpty();
    }

    [Fact]
    public void GetReason_WithZeroPercent_ShouldReturnMustBeGreaterThanZero()
    {
        // Arrange - Use GetPercentReason method instead
        var percent = 0m;

        // Act
        var reason = _specification.GetPercentReason(percent);

        // Assert
        reason.Should().Contain("must be greater than 0 and less than 100");
    }

    [Fact]
    public void GetReason_WithNegativePercent_ShouldReturnMustBeGreaterThanZero()
    {
        // Arrange - Use GetPercentReason method instead
        var percent = -10m;

        // Act
        var reason = _specification.GetPercentReason(percent);

        // Assert
        reason.Should().Contain("must be greater than 0 and less than 100");
    }

    [Fact]
    public void GetReason_With100Percent_ShouldReturnMustBeLessThan100()
    {
        // Arrange - Use GetPercentReason method instead
        var percent = 100m;

        // Act
        var reason = _specification.GetPercentReason(percent);

        // Assert
        reason.Should().Contain("must be greater than 0 and less than 100");
    }

    [Fact]
    public void GetReason_WithPercentOver100_ShouldReturnMustBeLessThan100()
    {
        // Arrange - Use GetPercentReason method instead
        var percent = 150m;

        // Act
        var reason = _specification.GetPercentReason(percent);

        // Assert
        reason.Should().Contain("must be greater than 0 and less than 100");
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void IsSatisfiedBy_BoundaryValues_ShouldBehaviorCorrectly()
    {
        // Arrange - Just above 0
        var justAboveZero = new Progression(DateTime.Now, 0.01m);
        
        // Arrange - Just below 100
        var justBelow100 = new Progression(DateTime.Now, 99.99m);

        // Act & Assert - Valid progressions
        _specification.IsSatisfiedBy(justAboveZero).Should().BeTrue();
        _specification.IsSatisfiedBy(justBelow100).Should().BeTrue();
        
        // Act & Assert - Invalid percents using IsValidPercent
        _specification.IsValidPercent(0m).Should().BeFalse();
        _specification.IsValidPercent(100m).Should().BeFalse();
    }

    #endregion

    #region Date Independence Tests

    [Fact]
    public void IsSatisfiedBy_ShouldNotDependOnDate()
    {
        // Arrange
        var progression1 = new Progression(DateTime.MinValue, 50m);
        var progression2 = new Progression(DateTime.MaxValue, 50m);
        var progression3 = new Progression(new DateTime(2024, 1, 1), 50m);

        // Act & Assert
        _specification.IsSatisfiedBy(progression1).Should().BeTrue();
        _specification.IsSatisfiedBy(progression2).Should().BeTrue();
        _specification.IsSatisfiedBy(progression3).Should().BeTrue();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Specification_RealWorldScenario_ShouldValidateCorrectly()
    {
        // Valid progressions
        var validProgressions = new[]
        {
            new Progression(DateTime.Now, 10m),
            new Progression(DateTime.Now.AddDays(1), 25m),
            new Progression(DateTime.Now.AddDays(2), 30m)
        };

        foreach (var progression in validProgressions)
        {
            _specification.IsSatisfiedBy(progression).Should().BeTrue();
            _specification.GetReason(progression).Should().BeEmpty();
        }

        // Invalid percents - test using IsValidPercent instead of creating invalid objects
        var invalidPercents = new[] { 0m, 100m, -5m, 150m };

        foreach (var percent in invalidPercents)
        {
            _specification.IsValidPercent(percent).Should().BeFalse();
            _specification.GetPercentReason(percent).Should().NotBeEmpty();
        }
    }

    #endregion

    #region Decimal Precision Tests

    [Theory]
    [InlineData(0.0001)]
    [InlineData(0.001)]
    [InlineData(0.01)]
    [InlineData(99.99)]
    [InlineData(99.999)]
    [InlineData(99.9999)]
    public void IsSatisfiedBy_WithHighPrecisionDecimals_ShouldHandleCorrectly(decimal percent)
    {
        // Arrange
        var progression = new Progression(DateTime.Now, percent);

        // Act
        var result = _specification.IsSatisfiedBy(progression);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
