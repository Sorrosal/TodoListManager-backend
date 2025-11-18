// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Infrastructure.Services;

namespace TodoListManager.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for PasswordHasher service.
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    #region HashPassword Tests

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void HashPassword_SamePWord_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "SamePassword";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_WithInvalidPassword_ShouldThrowArgumentException(string password)
    {
        // Act
        Action act = () => _passwordHasher.HashPassword(password);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "MyPassword123";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword";
        var wrongPassword = "WrongPassword";
        var hash = _passwordHasher.HashPassword(correctPassword);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_WithInvalidPassword_ShouldThrowArgumentException(string password)
    {
        // Arrange
        var hash = "some_hash";

        // Act
        Action act = () => _passwordHasher.VerifyPassword(password, hash);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_WithInvalidHash_ShouldThrowArgumentException(string hash)
    {
        // Act
        Action act = () => _passwordHasher.VerifyPassword("password", hash);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Integration Tests

    [Theory]
    [InlineData("simple")]
    [InlineData("Complex!@#123")]
    [InlineData("Very Long Password With Many Characters 12345")]
    public void HashAndVerify_WithVariousPasswords_ShouldWorkCorrectly(string password)
    {
        // Act
        var hash = _passwordHasher.HashPassword(password);
        var isValid = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    #endregion
}
