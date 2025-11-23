// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Services;
using TodoListManager.Domain.Specifications;

namespace TodoListManager.Infrastructure.Factories;

/// <summary>
/// Factory implementation for creating TodoList aggregate instances.
/// Encapsulates the creation logic and dependencies.
/// </summary>
public class TodoListFactory : ITodoListFactory
{
    private readonly ICategoryValidator _categoryValidator;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;

    /// <summary>
    /// Initializes a new instance of <see cref="TodoListFactory"/>.
    /// </summary>
    /// <param name="categoryValidator">The category validator service.</param>
    /// <param name="canModifySpecification">The specification to check if an item can be modified.</param>
    /// <param name="validProgressionSpecification">The specification to validate progressions.</param>
    public TodoListFactory(
        ICategoryValidator categoryValidator,
        CanModifyTodoItemSpecification canModifySpecification,
        ValidProgressionSpecification validProgressionSpecification)
    {
        _categoryValidator = categoryValidator ?? throw new ArgumentNullException(nameof(categoryValidator));
        _canModifySpecification = canModifySpecification ?? throw new ArgumentNullException(nameof(canModifySpecification));
        _validProgressionSpecification = validProgressionSpecification ?? throw new ArgumentNullException(nameof(validProgressionSpecification));
    }

    /// <summary>
    /// Creates a new TodoList aggregate instance with all required dependencies.
    /// </summary>
    /// <returns>A new TodoList aggregate instance.</returns>
    public TodoList Create()
    {
        return new TodoList(_categoryValidator, _canModifySpecification, _validProgressionSpecification);
    }
}
