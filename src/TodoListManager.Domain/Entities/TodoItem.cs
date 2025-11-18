// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Entities;

/// <summary>
/// Represents a todo item with progression tracking capabilities.
/// </summary>
public class TodoItem
{
    public int Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    private readonly List<Progression> _progressions;
    public IReadOnlyList<Progression> Progressions => _progressions.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this item is completed (100% progress).
    /// </summary>
    public bool IsCompleted => GetTotalProgress() >= 100m;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoItem"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The title of the todo item.</param>
    /// <param name="description">The description of the todo item.</param>
    /// <param name="category">The category of the todo item.</param>
    public TodoItem(int id, string title, string description, string category)
    {
        Id = id;
        Title = title;
        Description = description;
        Category = category;
        _progressions = new List<Progression>();
    }

    /// <summary>
    /// Updates the description of the todo item.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void UpdateDescription(string description)
    {
        Description = description;
    }

    /// <summary>
    /// Adds a progression entry to the todo item.
    /// </summary>
    /// <param name="date">The date of the progression.</param>
    /// <param name="percent">The percentage of progress completed.</param>
    public void AddProgression(DateTime date, decimal percent)
    {
        _progressions.Add(new Progression(date, percent));
    }

    /// <summary>
    /// Calculates the total progress percentage.
    /// </summary>
    /// <returns>The cumulative progress percentage.</returns>
    public decimal GetTotalProgress()
    {
        return _progressions.Sum(p => p.Percent);
    }

    /// <summary>
    /// Gets the date of the most recent progression entry.
    /// </summary>
    /// <returns>The last progression date, or null if no progressions exist.</returns>
    public DateTime? GetLastProgressionDate()
    {
        return _progressions.Count > 0 
            ? _progressions.Max(p => p.Date) 
            : null;
    }
}
