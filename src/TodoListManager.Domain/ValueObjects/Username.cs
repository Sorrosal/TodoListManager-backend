// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.ValueObjects;

/// <summary>
/// Value object representing a username with validation rules.
/// </summary>
public sealed class Username : IEquatable<Username>
{
    private const int MinLength = 3;
    private const int MaxLength = 50;

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Username with validation.
    /// </summary>
    /// <param name="value">The username string.</param>
    /// <returns>A validated Username value object.</returns>
    /// <exception cref="ArgumentException">Thrown when username is invalid.</exception>
    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username cannot be empty.", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length < MinLength)
            throw new ArgumentException($"Username must be at least {MinLength} characters.", nameof(value));

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Username cannot exceed {MaxLength} characters.", nameof(value));

        if (!IsValidFormat(trimmed))
            throw new ArgumentException("Username can only contain letters, numbers, and underscores.", nameof(value));

        return new Username(trimmed);
    }

    private static bool IsValidFormat(string value)
    {
        // Username can only contain alphanumeric characters and underscores
        return value.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    public override bool Equals(object? obj)
    {
        return obj is Username other && Equals(other);
    }

    public bool Equals(Username? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    /// <summary>
    /// Returns the string representation of the Username.
    /// </summary>
    /// <returns>The username value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a Username to a string.
    /// </summary>
    /// <param name="username">The Username to convert.</param>
    public static implicit operator string(Username username) => username.Value;

    /// <summary>
    /// Determines whether two Username instances are equal.
    /// </summary>
    public static bool operator ==(Username? left, Username? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Username instances are not equal.
    /// </summary>
    public static bool operator !=(Username? left, Username? right)
    {
        return !(left == right);
    }
}
