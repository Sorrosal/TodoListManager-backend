// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Repositories;

namespace TodoListManager.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of the todo list repository.
/// </summary>
public class InMemoryTodoListRepository : ITodoListRepository
{
    private int _currentId = 0;
    private readonly List<string> _categories = new()
    {
        "Work",
        "Personal",
        "Shopping",
        "Health",
        "Education",
        "Finance",
        "Home"
    };

    /// <summary>
    /// Generates the next available ID for a todo item.
    /// </summary>
    /// <returns>The next available ID.</returns>
    public int GetNextId()
    {
        return ++_currentId;
    }

    /// <summary>
    /// Gets all valid categories for todo items.
    /// </summary>
    /// <returns>A list of valid category names.</returns>
    public List<string> GetAllCategories()
    {
        return _categories;
    }
}
