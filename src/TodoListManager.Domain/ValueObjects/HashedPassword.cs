// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.ValueObjects;

/// <summary>
/// Value object representing a hashed password.
/// Ensures type safety and prevents plain text passwords from being used accidentally.
/// </summary>
public sealed class HashedPassword : IEquatable<HashedPassword>
{
    public string Value { get; }

    private HashedPassword(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a HashedPassword from an already hashed string.
    /// </summary>
    /// <param name="hashedValue">The hashed password value.</param>
    /// <returns>A HashedPassword instance.</returns>
    /// <exception cref="ArgumentException">Thrown when hashedValue is null or empty.</exception>
    public static HashedPassword FromHash(string hashedValue)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
            throw new ArgumentException("Hashed password cannot be empty.", nameof(hashedValue));

        return new HashedPassword(hashedValue);
    }

    public override bool Equals(object? obj)
    {
        return obj is HashedPassword other && Equals(other);
    }

    public bool Equals(HashedPassword? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return "********";
    }

    public static implicit operator string(HashedPassword password)
    {
        return password?.Value ?? string.Empty;
    }

    public static bool operator ==(HashedPassword? left, HashedPassword? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(HashedPassword? left, HashedPassword? right)
    {
        return !(left == right);
    }
}
