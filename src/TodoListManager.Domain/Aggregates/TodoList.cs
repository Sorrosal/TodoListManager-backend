// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Domain.Aggregates;

public class TodoList : ITodoList
{
    private readonly Dictionary<int, TodoItem> _items;
    private readonly ITodoListRepository _repository;

    public TodoList(ITodoListRepository repository)
    {
        _items = new Dictionary<int, TodoItem>();
        _repository = repository;
    }

    public void AddItem(int id, string title, string description, string category)
    {
        ValidateCategory(category);

        var item = new TodoItem(id, title, description, category);
        _items[id] = item;
    }

    public void UpdateItem(int id, string description)
    {
        var item = GetItemOrThrow(id);

        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }

        item.UpdateDescription(description);
    }

    public void RemoveItem(int id)
    {
        var item = GetItemOrThrow(id);

        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }

        _items.Remove(id);
    }

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

    // Method to get all items for query purposes
    public IReadOnlyList<TodoItem> GetAllItems()
    {
        return _items.Values.OrderBy(i => i.Id).ToList();
    }
}
