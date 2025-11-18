// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.ValueObjects;

public class Progression
{
    public DateTime Date { get; private set; }
    public decimal Percent { get; private set; }

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
