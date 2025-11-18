// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Entities;

namespace TodoListManager.Domain.Repositories;

/// <summary>
/// Repository interface for managing users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public User? GetByUsername(string username);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public User? GetById(int id);

    /// <summary>
    /// Adds a new user.
    /// </summary>
    /// <param name="user">The user to add.</param>
    public void Add(User user);

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>A collection of all users.</returns>
    public IEnumerable<User> GetAll();
}
