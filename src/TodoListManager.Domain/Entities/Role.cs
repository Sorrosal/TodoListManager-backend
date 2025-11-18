// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Entities;

/// <summary>
/// Represents a role in the system.
/// </summary>
public class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<Permission> Permissions { get; private set; }

    private Role() 
    { 
        Name = string.Empty;
        Description = string.Empty;
        Permissions = new List<Permission>();
    }

    public Role(int id, string name, string description, List<Permission> permissions)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Permissions = permissions ?? new List<Permission>();
    }

    // Predefined roles
    public static Role Admin => new(1, "Admin", "Administrator with full permissions", new List<Permission>
    {
        Permission.TodoListRead,
        Permission.TodoListCreate,
        Permission.TodoListUpdate,
        Permission.TodoListDelete,
        Permission.TodoListManage
    });

    public static Role User => new(2, "User", "Regular user with basic permissions", new List<Permission>
    {
        Permission.TodoListRead,
        Permission.TodoListCreate,
        Permission.TodoListUpdate
    });
}
