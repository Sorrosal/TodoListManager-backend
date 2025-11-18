// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Domain.Aggregates;

/// <summary>
/// Manages a collection of todo items with business rules and validation.
/// </summary>
public class TodoList : ITodoList
{
    private readonly Dictionary<int, TodoItem> _items;
    private readonly ITodoListRepository _repository;

    public TodoList(ITodoListRepository repository)
    {
        _items = new Dictionary<int, TodoItem>();
        _repository = repository;
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
        ValidateCategory(category);

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

        // Validate percent is valid (greater than 0 and less than 100)
        if (percent <= 0 || percent >= 100)
        {
            throw new InvalidProgressionException("Percent must be greater than 0 and less than 100.");
        }

        // Validate date is greater than all existing progression dates
        var lastDate = item.GetLastProgressionDate();
        if (lastDate.HasValue && dateTime <= lastDate.Value)
        {
            throw new InvalidProgressionException("The progression date must be greater than all existing progression dates.");
        }

        // Validate that adding this percent won't exceed 100%
        var currentTotal = item.GetTotalProgress();
        if (currentTotal + percent > 100m)
        {
            throw new InvalidProgressionException($"Adding {percent}% would exceed 100% total progress. Current progress: {currentTotal}%");
        }

        item.AddProgression(dateTime, percent);
    }

    /// <summary>
    /// Prints all todo items to the console with their progression details.
    /// </summary>
    public void PrintItems()
    {
        var sortedItems = _items.Values.OrderBy(i => i.Id).ToList();

        foreach (var item in sortedItems)
        {
            PrintTodoItem(item);
        }
    }

    private void PrintTodoItem(TodoItem item)
    {
        Console.WriteLine($"{item.Id}) {item.Title} - {item.Description} ({item.Category}) Completed:{item.IsCompleted}");

        decimal cumulativePercent = 0;
        foreach (var progression in item.Progressions)
        {
            cumulativePercent += progression.Percent;
            var progressBar = GenerateProgressBar(cumulativePercent);
            Console.WriteLine($"{progression.Date} - {cumulativePercent}% {progressBar}");
        }

        if (item.Progressions.Count > 0)
        {
            Console.WriteLine(); // Empty line between items
        }
    }

    private string GenerateProgressBar(decimal percent)
    {
        const int totalBars = 50;
        var filledBars = (int)Math.Round(percent / 100m * totalBars);
        var emptyBars = totalBars - filledBars;

        var filled = new string('O', filledBars);
        var empty = new string(' ', emptyBars);

        return $"|{filled}{empty}|";
    }

    private TodoItem GetItemOrThrow(int id)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            throw new TodoItemNotFoundException(id);
        }

        return item;
    }

    private void ValidateCategory(string category)
    {
        var validCategories = _repository.GetAllCategories();
        if (!validCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidCategoryException(category);
        }
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
