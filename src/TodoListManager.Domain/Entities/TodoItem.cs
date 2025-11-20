// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Common;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Entities;

/// <summary>
/// Represents a todo item with progression tracking capabilities.
/// Entity with identity and encapsulated business logic.
/// </summary>
public class TodoItem : BaseEntity
{
    private string _title;
    private string _description;
    private string _category;
    private readonly List<Progression> _progressions;

    public string Title => _title;
    public string Description => _description;
    public string Category => _category;
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
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Id = id;
        _title = title;
        _description = description ?? string.Empty;
        _category = category ?? throw new ArgumentNullException(nameof(category));
        _progressions = new List<Progression>();
    }

    /// <summary>
    /// Updates the description of the todo item.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void UpdateDescription(string description)
    {
        _description = description ?? string.Empty;
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
