// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid progression is registered for a todo item.
/// </summary>
public class InvalidProgressionException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidProgressionException"/> class.
    /// </summary>
    /// <param name="message">The error message explaining why the progression is invalid.</param>
    public InvalidProgressionException(string message) : base(message)
    {
    }
}
