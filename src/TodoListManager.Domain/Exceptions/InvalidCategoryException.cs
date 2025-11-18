// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

public class InvalidCategoryException : DomainException
{
    public InvalidCategoryException(string category) : base($"Category '{category}' is not valid.")
    {
    }
}
