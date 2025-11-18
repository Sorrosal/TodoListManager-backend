// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of the user repository with seeded admin user.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private int _nextId = 2;

    public InMemoryUserRepository()
    {
        _users = new List<User>();
        SeedAdminUser();
    }

    private void SeedAdminUser()
    {
        // Create admin user with username "admin" and password "admin"
        // Password hash for "admin" using BCrypt
        var adminUser = new User(
            1,
            "admin",
            BCrypt.Net.BCrypt.HashPassword("admin"),
            new List<Role> { Role.Admin }
        );

        _users.Add(adminUser);
    }

    public User? GetByUsername(string username)
    {
        return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public User? GetById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public void Add(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (_users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"User with username '{user.Username}' already exists.");

        var newUser = new User(_nextId++, user.Username, user.PasswordHash, user.Roles);
        _users.Add(newUser);
    }

    public IEnumerable<User> GetAll()
    {
        return _users.AsReadOnly();
    }
}
