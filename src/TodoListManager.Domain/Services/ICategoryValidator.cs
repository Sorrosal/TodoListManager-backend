// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Services;

/// <summary>
/// Domain service for category validation.
/// </summary>
public interface ICategoryValidator
{
    /// <summary>
    /// Validates if a category is valid according to business rules.
    /// </summary>
    /// <param name="category">The category to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValidCategory(string category);

    /// <summary>
    /// Gets all valid categories.
    /// </summary>
    /// <returns>A collection of valid category names.</returns>
    public IReadOnlyCollection<string> GetValidCategories();
}
