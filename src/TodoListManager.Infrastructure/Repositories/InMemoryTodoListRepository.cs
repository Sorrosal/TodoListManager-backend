// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Repositories;

namespace TodoListManager.Infrastructure.Repositories;

public class InMemoryTodoListRepository : ITodoListRepository
{
    private int _currentId = 0;
    private readonly List<string> _categories = new()
    {
        "Work",
        "Personal",
        "Shopping",
        "Health",
        "Education",
        "Finance",
        "Home"
    };

    public int GetNextId()
    {
        return ++_currentId;
    }

    public List<string> GetAllCategories()
    {
        return _categories;
    }
}
