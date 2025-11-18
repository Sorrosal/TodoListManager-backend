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
}
