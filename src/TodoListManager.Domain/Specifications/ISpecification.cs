// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Specifications;

/// <summary>
/// Specification pattern interface for encapsulating business rules.
/// </summary>
/// <typeparam name="T">The type to evaluate.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Checks if the candidate satisfies the specification.
    /// </summary>
    /// <param name="candidate">The object to evaluate.</param>
    /// <returns>True if satisfied, false otherwise.</returns>
    public bool IsSatisfiedBy(T candidate);

    /// <summary>
    /// Combines this specification with another using AND logic.
    /// </summary>
    public ISpecification<T> And(ISpecification<T> other);

    /// <summary>
    /// Combines this specification with another using OR logic.
    /// </summary>
    public ISpecification<T> Or(ISpecification<T> other);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    public ISpecification<T> Not();
}
