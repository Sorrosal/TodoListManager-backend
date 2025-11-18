// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Entities;

/// <summary>
/// Represents a permission in the system.
/// </summary>
public class Permission
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private Permission() 
    { 
        Name = string.Empty;
        Description = string.Empty;
    }

    public Permission(int id, string name, string description)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    // Common permissions
    public static Permission TodoListRead => new(1, "TodoList.Read", "Read todo list items");
    public static Permission TodoListCreate => new(2, "TodoList.Create", "Create new todo list items");
    public static Permission TodoListUpdate => new(3, "TodoList.Update", "Update existing todo list items");
    public static Permission TodoListDelete => new(4, "TodoList.Delete", "Delete todo list items");
    public static Permission TodoListManage => new(5, "TodoList.Manage", "Full management of todo list items");
}
