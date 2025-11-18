// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException("A successful result cannot have an error.");
        
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new Result(true, string.Empty);
    
    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <returns>A failed result.</returns>
    public static Result Failure(string error) => new Result(false, error);
    
    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <returns>A successful result containing the value.</returns>
    public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);
    
    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure<T>(string error) => new Result<T>(default!, false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error) 
        : base(isSuccess, error)
    {
        Value = value;
    }
}
