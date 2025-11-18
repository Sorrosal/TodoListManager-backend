// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Repositories;

/// <summary>
/// Defines the contract for todo list repository operations.
/// </summary>
public interface ITodoListRepository
{
    /// <summary>
    /// Generates the next available ID for a todo item.
    /// </summary>
    /// <returns>The next available ID.</returns>
    public int GetNextId();

    /// <summary>
    /// Gets all valid categories for todo items.
    /// </summary>
    /// <returns>A list of valid category names.</returns>
    public List<string> GetAllCategories();
}
