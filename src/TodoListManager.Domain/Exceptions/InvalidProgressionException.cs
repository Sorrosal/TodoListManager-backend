// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Exceptions;

public class InvalidProgressionException : DomainException
{
    public InvalidProgressionException(string message) : base(message)
    {
    }
}
