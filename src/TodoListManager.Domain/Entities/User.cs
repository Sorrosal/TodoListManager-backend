// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Entities;

/// <summary>
/// User aggregate root - manages user identity, authentication, and authorization.
/// </summary>
public class User
{
    public int Id { get; private set; }
    public Username Username { get; private set; }
    public HashedPassword PasswordHash { get; private set; }
    public List<Role> Roles { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Required by EF Core
    private User() 
    { 
        Username = null!;
        PasswordHash = null!;
        Roles = new List<Role>();
    }

    /// <summary>
    /// Creates a new User aggregate.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="username">Username value object.</param>
    /// <param name="passwordHash">Hashed password value object.</param>
    /// <param name="roles">User roles with permissions.</param>
    private User(int id, Username username, HashedPassword passwordHash, List<Role> roles)
    {
        if (id <= 0)
            throw new ArgumentException("User ID must be positive.", nameof(id));

        if (roles == null || !roles.Any())
            throw new ArgumentException("User must have at least one role.", nameof(roles));

        Id = id;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Roles = roles;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new User.
    /// </summary>
    public static User Create(int id, string username, string hashedPassword, List<Role> roles)
    {
        var usernameVo = Username.Create(username);
        var passwordVo = HashedPassword.FromHash(hashedPassword);

        return new User(id, usernameVo, passwordVo, roles);
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        return Roles.Any(role => role.Permissions.Any(p => p.Name == permission));
    }

    /// <summary>
    /// Domain logic: Checks if user has a specific role.
    /// </summary>
    public bool HasRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        return Roles.Any(role => role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    public void ChangePassword(HashedPassword newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
    }

    /// <summary>
    /// Adds a role to the user.
    /// </summary>
    public void AddRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (Roles.Any(r => r.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"User already has role '{role.Name}'.");

        Roles.Add(role);
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        var role = Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (role == null)
            throw new InvalidOperationException($"User does not have role '{roleName}'.");

        if (Roles.Count == 1)
            throw new InvalidOperationException("Cannot remove the last role. User must have at least one role.");

        Roles.Remove(role);
    }
}
