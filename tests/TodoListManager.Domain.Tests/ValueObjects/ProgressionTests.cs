// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Tests.ValueObjects;

/// <summary>
/// Unit tests for Progression value object following best practices.
/// </summary>
public class ProgressionTests
{
    #region Creation Tests

    [Fact]
    public void Constructor_WithValidValues_ShouldCreateProgression()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var percent = 25.5m;

        // Act
        var progression = new Progression(date, percent);

        // Assert
        progression.Should().NotBeNull();
        progression.Date.Should().Be(date);
        progression.Percent.Should().Be(percent);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(10.0)]
    [InlineData(50.5)]
    [InlineData(99.9)]
    public void Constructor_WithVariousValidPercentages_ShouldSucceed(decimal percent)
    {
        // Arrange
        var date = DateTime.Now;

        // Act
        var progression = new Progression(date, percent);

        // Assert
        progression.Percent.Should().Be(percent);
    }

    [Fact]
    public void Constructor_WithMinDate_ShouldSucceed()
    {
        // Arrange
        var minDate = DateTime.MinValue;
        var percent = 10m;

        // Act
        var progression = new Progression(minDate, percent);

        // Assert
        progression.Date.Should().Be(minDate);
    }

    [Fact]
    public void Constructor_WithMaxDate_ShouldSucceed()
    {
        // Arrange
        var maxDate = DateTime.MaxValue;
        var percent = 10m;

        // Act
        var progression = new Progression(maxDate, percent);

        // Assert
        progression.Date.Should().Be(maxDate);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameDateAndPercent_ShouldReturnTrue()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15, 10, 30, 0);
        var percent = 25.5m;
        var progression1 = new Progression(date, percent);
        var progression2 = new Progression(date, percent);

        // Act
        var result = progression1.Equals(progression2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentDate_ShouldReturnFalse()
    {
        // Arrange
        var progression1 = new Progression(new DateTime(2024, 1, 15), 25m);
        var progression2 = new Progression(new DateTime(2024, 1, 16), 25m);

        // Act
        var result = progression1.Equals(progression2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentPercent_ShouldReturnFalse()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var progression1 = new Progression(date, 25m);
        var progression2 = new Progression(date, 30m);

        // Act
        var result = progression1.Equals(progression2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, 25m);

        // Act
        var result = progression.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var progression = new Progression(DateTime.Now, 25m);
        var other = new object();

        // Act
        var result = progression.Equals(other);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15, 10, 30, 0);
        var percent = 25.5m;
        var progression1 = new Progression(date, percent);
        var progression2 = new Progression(date, percent);

        // Act & Assert
        progression1.GetHashCode().Should().Be(progression2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var progression1 = new Progression(DateTime.Now, 25m);
        var progression2 = new Progression(DateTime.Now.AddDays(1), 30m);

        // Act & Assert
        progression1.GetHashCode().Should().NotBe(progression2.GetHashCode());
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithZeroPercent_ShouldSucceed()
    {
        // Arrange
        var date = DateTime.Now;
        var percent = 0m;

        // Act
        var progression = new Progression(date, percent);

        // Assert
        // Constructor allows 0, but business logic (specification) considers it invalid
        progression.Percent.Should().Be(0m);
    }

    [Fact]
    public void Constructor_WithNegativePercent_ShouldThrowArgumentException()
    {
        // Arrange
        var date = DateTime.Now;
        var percent = -10m;

        // Act
        Action act = () => new Progression(date, percent);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Percent must be between 0 and 100*")
            .WithParameterName("percent");
    }

    [Fact]
    public void Constructor_WithPercentOver100_ShouldThrowArgumentException()
    {
        // Arrange
        var date = DateTime.Now;
        var percent = 150m;

        // Act
        Action act = () => new Progression(date, percent);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Percent must be between 0 and 100*")
            .WithParameterName("percent");
    }

    #endregion

    #region Collection Behavior Tests

    [Fact]
    public void Progressions_InList_ShouldMaintainEquality()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var percent = 25m;
        var progression1 = new Progression(date, percent);
        var progression2 = new Progression(date, percent);
        var list = new List<Progression> { progression1 };

        // Act
        var contains = list.Contains(progression2);

        // Assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void Progressions_InHashSet_ShouldPreventDuplicates()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var percent = 25m;
        var progression1 = new Progression(date, percent);
        var progression2 = new Progression(date, percent);
        var hashSet = new HashSet<Progression> { progression1 };

        // Act
        var added = hashSet.Add(progression2);

        // Assert
        added.Should().BeFalse();
        hashSet.Should().HaveCount(1);
    }

    #endregion
}
