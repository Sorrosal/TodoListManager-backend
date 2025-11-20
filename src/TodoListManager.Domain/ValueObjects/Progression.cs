// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.ValueObjects;

/// <summary>
/// Represents a progress entry for a todo item at a specific date.
/// Value object - immutable and defined by its attributes.
/// </summary>
public sealed class Progression : IEquatable<Progression>
{
    public DateTime Date { get; }
    public decimal Percent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Progression"/> class.
    /// </summary>
    /// <param name="date">The date of the progression.</param>
    /// <param name="percent">The percentage of progress completed.</param>
    public Progression(DateTime date, decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentException("Percent must be between 0 and 100.", nameof(percent));

        Date = date;
        Percent = percent;
    }

    public override bool Equals(object? obj)
    {
        return obj is Progression other && Equals(other);
    }

    public bool Equals(Progression? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Date == other.Date && Percent == other.Percent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Date, Percent);
    }

    public static bool operator ==(Progression? left, Progression? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Progression? left, Progression? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{Percent}% on {Date:yyyy-MM-dd}";
    }
}
