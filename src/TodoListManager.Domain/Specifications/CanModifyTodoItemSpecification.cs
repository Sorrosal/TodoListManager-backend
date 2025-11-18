// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;

namespace TodoListManager.Domain.Specifications;

/// <summary>
/// Specification to determine if a TodoItem can be modified.
/// </summary>
public class CanModifyTodoItemSpecification : Specification<TodoItem>
{
    private const decimal MaxProgressThreshold = 50m;

    public override bool IsSatisfiedBy(TodoItem candidate)
    {
        if (candidate == null)
            throw new ArgumentNullException(nameof(candidate));

        return candidate.GetTotalProgress() <= MaxProgressThreshold;
    }

    /// <summary>
    /// Gets the reason why modification is not allowed.
    /// </summary>
    public string GetReason(TodoItem item)
    {
        if (item == null)
            return "Item is null";

        if (!IsSatisfiedBy(item))
            return $"Item cannot be modified because it has {item.GetTotalProgress()}% progress (maximum allowed: {MaxProgressThreshold}%)";

        return string.Empty;
    }
}
