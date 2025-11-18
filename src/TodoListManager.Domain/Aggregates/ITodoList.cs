// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Aggregates;

/// <summary>
/// Defines the contract for managing a collection of todo items.
/// </summary>
public interface ITodoList
{
    /// <summary>
    /// Adds a new todo item to the list.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The title of the item.</param>
    /// <param name="description">The description of the item.</param>
    /// <param name="category">The category of the item.</param>
    void AddItem(int id, string title, string description, string category);

    /// <summary>
    /// Updates the description of an existing todo item.
    /// </summary>
    /// <param name="id">The unique identifier of the item to update.</param>
    /// <param name="description">The new description.</param>
    void UpdateItem(int id, string description);

    /// <summary>
    /// Removes a todo item from the list.
    /// </summary>
    /// <param name="id">The unique identifier of the item to remove.</param>
    void RemoveItem(int id);

    /// <summary>
    /// Registers a progression entry for a todo item.
    /// </summary>
    /// <param name="id">The unique identifier of the item.</param>
    /// <param name="dateTime">The date of the progression.</param>
    /// <param name="percent">The percentage of progress to add.</param>
    void RegisterProgression(int id, DateTime dateTime, decimal percent);

    /// <summary>
    /// Prints all todo items to the console.
    /// </summary>
    void PrintItems();
}
