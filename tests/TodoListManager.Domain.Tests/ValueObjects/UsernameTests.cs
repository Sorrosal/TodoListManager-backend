// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Tests.ValueObjects;

/// <summary>
/// Unit tests for Username value object following best practices:
/// - Arrange-Act-Assert pattern
/// - One assertion per test (with FluentAssertions)
/// - Descriptive test names
/// - Test both happy path and edge cases
/// </summary>
public class UsernameTests
{
    #region Creation Tests

    [Fact]
    public void Create_WithValidUsername_ShouldReturnUsernameInstance()
    {
        // Arrange
        var validUsername = "valid_user123";

        // Act
        var result = Username.Create(validUsername);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(validUsername);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("user_123")]
    [InlineData("User123")]
    [InlineData("test_user_name_123")]
    public void Create_WithValidFormats_ShouldSucceed(string validUsername)
    {
        // Act
        var result = Username.Create(validUsername);

        // Assert
        result.Value.Should().Be(validUsername);
    }

    [Fact]
    public void Create_WithUsernameContainingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var usernameWithSpaces = "  valid_user  ";

        // Act
        var result = Username.Create(usernameWithSpaces);

        // Assert
        result.Value.Should().Be("valid_user");
    }

    #endregion

    #region Validation Tests - Null/Empty

    [Fact]
    public void Create_WithNull_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => Username.Create(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be empty.*");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => Username.Create(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be empty.*");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => Username.Create("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be empty.*");
    }

    #endregion

    #region Validation Tests - Length

    [Fact]
    public void Create_WithTooShortUsername_ShouldThrowArgumentException()
    {
        // Arrange
        var shortUsername = "ab"; // Less than 3 characters

        // Act
        Action act = () => Username.Create(shortUsername);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username must be at least 3 characters.*");
    }

    [Fact]
    public void Create_WithTooLongUsername_ShouldThrowArgumentException()
    {
        // Arrange
        var longUsername = new string('a', 51); // More than 50 characters

        // Act
        Action act = () => Username.Create(longUsername);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot exceed 50 characters.*");
    }

    [Fact]
    public void Create_WithExactMinimumLength_ShouldSucceed()
    {
        // Arrange
        var minLengthUsername = "abc"; // Exactly 3 characters

        // Act
        var result = Username.Create(minLengthUsername);

        // Assert
        result.Value.Should().Be(minLengthUsername);
    }

    [Fact]
    public void Create_WithExactMaximumLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthUsername = new string('a', 50); // Exactly 50 characters

        // Act
        var result = Username.Create(maxLengthUsername);

        // Assert
        result.Value.Should().HaveLength(50);
    }

    #endregion

    #region Validation Tests - Format

    [Theory]
    [InlineData("user@name")]
    [InlineData("user name")]
    [InlineData("user-name")]
    [InlineData("user.name")]
    [InlineData("user#123")]
    [InlineData("user$test")]
    public void Create_WithInvalidCharacters_ShouldThrowArgumentException(string invalidUsername)
    {
        // Act
        Action act = () => Username.Create(invalidUsername);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username can only contain letters, numbers, and underscores.*");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var username1 = Username.Create("testuser");
        var username2 = Username.Create("testuser");

        // Act
        var result = username1.Equals(username2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        var username1 = Username.Create("TestUser");
        var username2 = Username.Create("testuser");

        // Act
        var result = username1.Equals(username2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var username1 = Username.Create("user1");
        var username2 = Username.Create("user2");

        // Act
        var result = username1.Equals(username2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var username = Username.Create("testuser");

        // Act
        var result = username.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualsOperator_WithEqualUsernames_ShouldReturnTrue()
    {
        // Arrange
        var username1 = Username.Create("testuser");
        var username2 = Username.Create("testuser");

        // Act
        var result = username1 == username2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void NotEqualsOperator_WithDifferentUsernames_ShouldReturnTrue()
    {
        // Arrange
        var username1 = Username.Create("user1");
        var username2 = Username.Create("user2");

        // Act
        var result = username1 != username2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var username1 = Username.Create("testuser");
        var username2 = Username.Create("testuser");

        // Act & Assert
        username1.GetHashCode().Should().Be(username2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentCase_ShouldReturnSameHashCode()
    {
        // Arrange
        var username1 = Username.Create("TestUser");
        var username2 = Username.Create("testuser");

        // Act & Assert
        username1.GetHashCode().Should().Be(username2.GetHashCode());
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var username = Username.Create("testuser");

        // Act
        var result = username.ToString();

        // Assert
        result.Should().Be("testuser");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var username = Username.Create("testuser");

        // Act
        string result = username;

        // Assert
        result.Should().Be("testuser");
    }

    #endregion
}
