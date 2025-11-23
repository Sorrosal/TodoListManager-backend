// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Services;

namespace TodoListManager.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of category validation using a static list.
/// </summary>
public sealed class CategoryValidator : ICategoryValidator
{
    private static readonly string[] ValidCategories = new[]
    {
        "Work",
        "Personal",
        "Education",
        "Health",
        "Finance",
        "Other"
    };

    public bool IsValidCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;

        return ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> GetValidCategories()
    {
        return ValidCategories.ToList().AsReadOnly();
    }
}
