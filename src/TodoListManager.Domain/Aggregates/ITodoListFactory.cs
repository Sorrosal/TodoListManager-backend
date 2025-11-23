// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Aggregates;

/// <summary>
/// Factory interface for creating TodoList aggregate instances.
/// </summary>
public interface ITodoListFactory
{
    /// <summary>
    /// Creates a new TodoList aggregate instance with all required dependencies.
    /// </summary>
    /// <returns>A new TodoList aggregate instance.</returns>
    TodoList Create();
}
