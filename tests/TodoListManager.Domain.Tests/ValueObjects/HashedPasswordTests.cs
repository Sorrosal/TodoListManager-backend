// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Tests.ValueObjects;

/// <summary>
/// Unit tests for HashedPassword value object following best practices.
/// </summary>
public class HashedPasswordTests
{
    #region Creation Tests

    [Fact]
    public void FromHash_WithValidHash_ShouldReturnHashedPasswordInstance()
    {
        // Arrange
        var validHash = "hashed_password_string_123";

        // Act
        var result = HashedPassword.FromHash(validHash);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(validHash);
    }

    [Theory]
    [InlineData("$2a$11$abcdefghijklmnopqrstuv")]
    [InlineData("simple_hash")]
    [InlineData("UPPERCASE_HASH")]
    public void FromHash_WithVariousFormats_ShouldSucceed(string hash)
    {
        // Act
        var result = HashedPassword.FromHash(hash);

        // Assert
        result.Value.Should().Be(hash);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void FromHash_WithNull_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => HashedPassword.FromHash(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Hashed password cannot be empty.*");
    }

    [Fact]
    public void FromHash_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => HashedPassword.FromHash(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Hashed password cannot be empty.*");
    }

    [Fact]
    public void FromHash_WithWhitespace_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => HashedPassword.FromHash("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Hashed password cannot be empty.*");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameHash_ShouldReturnTrue()
    {
        // Arrange
        var hash = "same_hash_value";
        var password1 = HashedPassword.FromHash(hash);
        var password2 = HashedPassword.FromHash(hash);

        // Act
        var result = password1.Equals(password2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentHash_ShouldReturnFalse()
    {
        // Arrange
        var password1 = HashedPassword.FromHash("hash1");
        var password2 = HashedPassword.FromHash("hash2");

        // Act
        var result = password1.Equals(password2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var password = HashedPassword.FromHash("some_hash");

        // Act
        var result = password.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualsOperator_WithEqualPasswords_ShouldReturnTrue()
    {
        // Arrange
        var hash = "same_hash";
        var password1 = HashedPassword.FromHash(hash);
        var password2 = HashedPassword.FromHash(hash);

        // Act
        var result = password1 == password2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void NotEqualsOperator_WithDifferentPasswords_ShouldReturnTrue()
    {
        // Arrange
        var password1 = HashedPassword.FromHash("hash1");
        var password2 = HashedPassword.FromHash("hash2");

        // Act
        var result = password1 != password2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var hash = "same_hash";
        var password1 = HashedPassword.FromHash(hash);
        var password2 = HashedPassword.FromHash(hash);

        // Act & Assert
        password1.GetHashCode().Should().Be(password2.GetHashCode());
    }

    #endregion

    #region Security Tests

    [Fact]
    public void ToString_ShouldNotRevealHash()
    {
        // Arrange
        var password = HashedPassword.FromHash("super_secret_hash");

        // Act
        var result = password.ToString();

        // Assert
        result.Should().Be("***");
        result.Should().NotContain("super_secret_hash");
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnActualValue()
    {
        // Arrange
        var hash = "actual_hash_value";
        var password = HashedPassword.FromHash(hash);

        // Act
        string result = password;

        // Assert
        result.Should().Be(hash);
    }

    #endregion
}
