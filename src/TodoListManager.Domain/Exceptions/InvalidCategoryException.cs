// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid category is specified for a todo item.
/// </summary>
public class InvalidCategoryException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidCategoryException"/> class.
    /// </summary>
    /// <param name="category">The invalid category name.</param>
    public InvalidCategoryException(string category) : base($"Category '{category}' is not valid.")
    {
    }
}
