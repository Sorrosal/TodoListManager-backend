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
        // Arrange
        var progression = new Progression(DateTime.Now, 0m);

        // Act
        var result = _specification.IsSatisfiedBy(progression);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithNegativePercent_ShouldReturnFalse()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, -10m);

        // Act
        var result = _specification.IsSatisfiedBy(progression);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithExactly100Percent_ShouldReturnFalse()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, 100m);

        // Act
        var result = _specification.IsSatisfiedBy(progression);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithPercentOver100_ShouldReturnFalse()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, 150m);

        // Act
        var result = _specification.IsSatisfiedBy(progression);

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
        // Arrange
        var progression = new Progression(DateTime.Now, percent);

        // Act
        var result = _specification.IsSatisfiedBy(progression);

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
        // Arrange
        var progression = new Progression(DateTime.Now, 0m);

        // Act
        var reason = _specification.GetReason(progression);

        // Assert
        reason.Should().Contain("must be greater than");
        reason.Should().Contain("0");
    }

    [Fact]
    public void GetReason_WithNegativePercent_ShouldReturnMustBeGreaterThanZero()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, -10m);

        // Act
        var reason = _specification.GetReason(progression);

        // Assert
        reason.Should().Contain("must be greater than");
        reason.Should().Contain("0");
    }

    [Fact]
    public void GetReason_With100Percent_ShouldReturnMustBeLessThan100()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, 100m);

        // Act
        var reason = _specification.GetReason(progression);

        // Assert
        reason.Should().Contain("must be less than");
        reason.Should().Contain("100");
    }

    [Fact]
    public void GetReason_WithPercentOver100_ShouldReturnMustBeLessThan100()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, 150m);

        // Act
        var reason = _specification.GetReason(progression);

        // Assert
        reason.Should().Contain("must be less than");
        reason.Should().Contain("100");
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
        
        // Arrange - Exactly at boundaries
        var exactlyZero = new Progression(DateTime.Now, 0m);
        var exactly100 = new Progression(DateTime.Now, 100m);

        // Act & Assert
        _specification.IsSatisfiedBy(justAboveZero).Should().BeTrue();
        _specification.IsSatisfiedBy(justBelow100).Should().BeTrue();
        _specification.IsSatisfiedBy(exactlyZero).Should().BeFalse();
        _specification.IsSatisfiedBy(exactly100).Should().BeFalse();
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

        // Invalid progressions
        var invalidProgressions = new[]
        {
            new Progression(DateTime.Now, 0m),
            new Progression(DateTime.Now, 100m),
            new Progression(DateTime.Now, -5m),
            new Progression(DateTime.Now, 150m)
        };

        foreach (var progression in invalidProgressions)
        {
            _specification.IsSatisfiedBy(progression).Should().BeFalse();
            _specification.GetReason(progression).Should().NotBeEmpty();
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
