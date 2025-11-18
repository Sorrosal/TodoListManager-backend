// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.ValueObjects;

/// <summary>
/// Represents a progress entry for a todo item at a specific date.
/// </summary>
public class Progression
{
    public DateTime Date { get; private set; }
    public decimal Percent { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Progression"/> class.
    /// </summary>
    /// <param name="date">The date of the progression.</param>
    /// <param name="percent">The percentage of progress completed.</param>
    public Progression(DateTime date, decimal percent)
    {
        Date = date;
        Percent = percent;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Progression other)
            return false;

        return Date == other.Date && Percent == other.Percent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Date, Percent);
    }
}
