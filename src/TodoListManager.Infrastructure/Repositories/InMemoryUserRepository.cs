// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;

namespace TodoListManager.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of the user repository with seeded admin user.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly int _nextId = 2;

    /// <summary>
    /// Constructor with dependency injection.
    /// </summary>
    public InMemoryUserRepository(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _users = new List<User>();
        SeedAdminUser();
    }

    /// <summary>
    /// Seeds the initial admin user.
    /// </summary>
    private void SeedAdminUser()
    {
        // Create admin user with username "admin" and password "admin"
        // Use domain service for password hashing
        var hashedPassword = _passwordHasher.HashPassword("admin");

        var adminUser = User.Create(
            1,
            "admin",
            hashedPassword,
            new List<Role> { Role.Admin }
        );

        _users.Add(adminUser);
    }

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    public User? GetByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        return _users.FirstOrDefault(u =>
            u.Username.Value.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    public User? GetById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    public void Add(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Enforce unique username constraint
        if (_users.Any(u => u.Username.Value.Equals(user.Username.Value, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"User with username '{user.Username}' already exists.");

        _users.Add(user);
    }

    /// <summary>
    /// Gets all users.
    /// Returns read-only collection to prevent external modification.
    /// </summary>
    public IEnumerable<User> GetAll()
    {
        return _users.AsReadOnly();
    }
}
