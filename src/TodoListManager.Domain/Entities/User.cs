// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public List<Role> Roles { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() 
    { 
        Username = string.Empty;
        PasswordHash = string.Empty;
        Roles = new List<Role>();
    }

    public User(int id, string username, string passwordHash, List<Role> roles)
    {
        Id = id;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Roles = roles ?? new List<Role>();
        CreatedAt = DateTime.UtcNow;
    }

    public bool HasPermission(string permission)
    {
        return Roles.Any(role => role.Permissions.Any(p => p.Name == permission));
    }

    public bool HasRole(string roleName)
    {
        return Roles.Any(role => role.Name == roleName);
    }
}
