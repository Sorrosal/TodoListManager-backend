// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Specifications;

/// <summary>
/// Specification to determine if a progression is valid.
/// </summary>
public class ValidProgressionSpecification : Specification<Progression>
{
    private const decimal MinPercent = 0m;
    private const decimal MaxPercent = 100m;

    public override bool IsSatisfiedBy(Progression candidate)
    {
        if (candidate == null)
            return false;

        return candidate.Percent is > MinPercent and < MaxPercent;
    }

    /// <summary>
    /// Validates if a percent value is valid (between 0 and 100, exclusive).
    /// </summary>
    public bool IsValidPercent(decimal percent)
    {
        return percent is > MinPercent and < MaxPercent;
    }

    /// <summary>
    /// Validates if adding a new percent would exceed the maximum allowed total (100%).
    /// </summary>
    public bool WouldExceedMaxTotal(decimal currentTotal, decimal newPercent)
    {
        return currentTotal + newPercent > MaxPercent;
    }

    public string GetReason(Progression progression)
    {
        if (progression == null)
            return "Progression is null";

        if (progression.Percent <= MinPercent)
            return $"Percent must be greater than {MinPercent}";

        if (progression.Percent >= MaxPercent)
            return $"Percent must be less than {MaxPercent}";

        return string.Empty;
    }

    /// <summary>
    /// Gets the reason why a percent is invalid.
    /// </summary>
    public string GetPercentReason(decimal percent)
    {
        if (percent <= MinPercent)
            return $"Percent must be greater than {MinPercent}";

        if (percent >= MaxPercent)
            return $"Percent must be less than {MaxPercent}";

        return string.Empty;
    }

    /// <summary>
    /// Gets the reason why adding a percent would exceed the total.
    /// </summary>
    public string GetExceedsTotalReason(decimal currentTotal, decimal newPercent)
    {
        if (WouldExceedMaxTotal(currentTotal, newPercent))
            return $"Adding {newPercent}% would exceed {MaxPercent}% total progress. Current progress: {currentTotal}%";

        return string.Empty;
    }
}
