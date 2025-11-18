// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Entities;

public class TodoItem
{
    public int Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    private readonly List<Progression> _progressions;
    public IReadOnlyList<Progression> Progressions => _progressions.AsReadOnly();

    public bool IsCompleted => GetTotalProgress() >= 100m;

    public TodoItem(int id, string title, string description, string category)
    {
        Id = id;
        Title = title;
        Description = description;
        Category = category;
        _progressions = new List<Progression>();
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void AddProgression(DateTime date, decimal percent)
    {
        _progressions.Add(new Progression(date, percent));
    }

    public decimal GetTotalProgress()
    {
        return _progressions.Sum(p => p.Percent);
    }

    public DateTime? GetLastProgressionDate()
    {
        return _progressions.Count > 0 
            ? _progressions.Max(p => p.Date) 
            : null;
    }
}
