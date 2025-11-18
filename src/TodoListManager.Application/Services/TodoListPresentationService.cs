// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Aggregates;

namespace TodoListManager.Application.Services;

/// <summary>
/// Application service for presenting todo list items.
/// </summary>
public class TodoListPresentationService
{
    private readonly ITodoList _todoList;

    public TodoListPresentationService(ITodoList todoList)
    {
        _todoList = todoList ?? throw new ArgumentNullException(nameof(todoList));
    }

    /// <summary>
    /// Prints all todo items to the console with their progression details.
    /// This is presentation logic, not domain logic.
    /// </summary>
    public void PrintItems()
    {
        var items = _todoList.GetAllItems();

        foreach (var item in items)
        {
            PrintTodoItem(item);
        }
    }

    private void PrintTodoItem(Domain.Entities.TodoItem item)
    {
        Console.WriteLine($"{item.Id}) {item.Title} - {item.Description} ({item.Category}) Completed:{item.IsCompleted}");

        decimal cumulativePercent = 0;
        foreach (var progression in item.Progressions)
        {
            cumulativePercent += progression.Percent;
            var progressBar = GenerateProgressBar(cumulativePercent);
            Console.WriteLine($"{progression.Date:yyyy-MM-dd} - {cumulativePercent}% {progressBar}");
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
}
