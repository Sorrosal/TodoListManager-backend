// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Entities;

namespace TodoListManager.Domain.Repositories;

/// <summary>
/// Defines the contract for todo list repository operations.
/// </summary>
public interface ITodoListRepository
{
    /// <summary>
    /// Gets all valid categories for todo items.
    /// </summary>
    /// <returns>A list of valid category names.</returns>
    public List<string> GetAllCategories();

    // Domain-level operations
    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<List<TodoItem>> GetAllDomainItemsAsync(CancellationToken cancellationToken = default);
    public Task SaveAsync(TodoItem item, CancellationToken cancellationToken = default);
    public Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default);
    public Task<TodoList> GetAggregateAsync(CancellationToken cancellationToken = default);
}
