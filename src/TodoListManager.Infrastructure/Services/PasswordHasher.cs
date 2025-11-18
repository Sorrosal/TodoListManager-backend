// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Services;

namespace TodoListManager.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of password hashing using BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Hashed password cannot be null or empty.", nameof(hashedPassword));

        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
