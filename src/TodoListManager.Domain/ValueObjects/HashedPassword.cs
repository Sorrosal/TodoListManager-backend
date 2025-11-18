// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.ValueObjects;

/// <summary>
/// Value object representing a hashed password.
/// </summary>
public sealed class HashedPassword : IEquatable<HashedPassword>
{
    public string Value { get; }

    private HashedPassword(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a HashedPassword from an already hashed value.
    /// </summary>
    /// <param name="hashedValue">The hashed password string.</param>
    /// <returns>A HashedPassword value object.</returns>
    /// <exception cref="ArgumentException">Thrown when hashed value is invalid.</exception>
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
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => "***";

    public static implicit operator string(HashedPassword password) => password.Value;

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
