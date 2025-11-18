// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Services;

namespace TodoListManager.Domain.Aggregates;

/// <summary>
/// TodoList aggregate root - manages a collection of todo items with business rules.
/// </summary>
public class TodoList : ITodoList
{
    private readonly Dictionary<int, TodoItem> _items;
    private readonly ICategoryValidator _categoryValidator;

    public TodoList(ICategoryValidator categoryValidator)
    {
        _items = new Dictionary<int, TodoItem>();
        _categoryValidator = categoryValidator ?? throw new ArgumentNullException(nameof(categoryValidator));
    }

    /// <summary>
    /// Adds a new todo item to the list.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The title of the item.</param>
    /// <param name="description">The description of the item.</param>
    /// <param name="category">The category of the item.</param>
    /// <exception cref="InvalidCategoryException">Thrown when the category is not valid.</exception>
    public void AddItem(int id, string title, string description, string category)
    {
        // Domain validation through domain service
        if (!_categoryValidator.IsValidCategory(category))
        {
            throw new InvalidCategoryException(category);
        }

        var item = new TodoItem(id, title, description, category);
        _items[id] = item;
    }

    /// <summary>
    /// Updates the description of an existing todo item.
    /// </summary>
    /// <param name="id">The unique identifier of the item to update.</param>
    /// <param name="description">The new description.</param>
    /// <exception cref="TodoItemNotFoundException">Thrown when the item is not found.</exception>
    /// <exception cref="TodoItemCannotBeModifiedException">Thrown when the item has more than 50% progress.</exception>
    public void UpdateItem(int id, string description)
    {
        var item = GetItemOrThrow(id);

        // Business rule: Cannot modify items with more than 50% progress
        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }

        item.UpdateDescription(description);
    }

    /// <summary>
    /// Removes a todo item from the list.
    /// </summary>
    /// <param name="id">The unique identifier of the item to remove.</param>
    /// <exception cref="TodoItemNotFoundException">Thrown when the item is not found.</exception>
    /// <exception cref="TodoItemCannotBeModifiedException">Thrown when the item has more than 50% progress.</exception>
    public void RemoveItem(int id)
    {
        var item = GetItemOrThrow(id);

        // Business rule: Cannot remove items with more than 50% progress
        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }

        _items.Remove(id);
    }

    /// <summary>
    /// Registers a progression entry for a todo item with validation.
    /// </summary>
    /// <param name="id">The unique identifier of the item.</param>
    /// <param name="dateTime">The date of the progression.</param>
    /// <param name="percent">The percentage of progress to add.</param>
    /// <exception cref="TodoItemNotFoundException">Thrown when the item is not found.</exception>
    /// <exception cref="InvalidProgressionException">Thrown when the progression is invalid.</exception>
    public void RegisterProgression(int id, DateTime dateTime, decimal percent)
    {
        var item = GetItemOrThrow(id);

        // Business rule: Percent must be valid
        if (percent is <= 0 or >= 100)
        {
            throw new InvalidProgressionException("Percent must be greater than 0 and less than 100.");
        }

        // Business rule: Date must be greater than all existing progression dates
        var lastDate = item.GetLastProgressionDate();
        if (lastDate.HasValue && dateTime <= lastDate.Value)
        {
            throw new InvalidProgressionException("The progression date must be greater than all existing progression dates.");
        }

        // Business rule: Total progress cannot exceed 100%
        var currentTotal = item.GetTotalProgress();
        if (currentTotal + percent > 100m)
        {
            throw new InvalidProgressionException($"Adding {percent}% would exceed 100% total progress. Current progress: {currentTotal}%");
        }

        item.AddProgression(dateTime, percent);
    }

    private TodoItem GetItemOrThrow(int id)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            throw new TodoItemNotFoundException(id);
        }

        return item;
    }

    /// <summary>
    /// Gets all todo items sorted by ID.
    /// </summary>
    /// <returns>A read-only list of all todo items.</returns>
    public IReadOnlyList<TodoItem> GetAllItems()
    {
        return _items.Values.OrderBy(i => i.Id).ToList();
    }
}
