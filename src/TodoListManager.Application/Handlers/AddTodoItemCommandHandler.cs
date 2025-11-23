// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;
using TodoListManager.Domain.Specifications;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to add a new todo item.
/// </summary>
public sealed class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryValidator _categoryValidator;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;

    /// <summary>
    /// Initializes a new instance of <see cref="AddTodoItemCommandHandler"/>.
    /// </summary>
    /// <param name="repository">The todo list repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="categoryValidator">The category validator service.</param>
    /// <param name="canModifySpecification">The specification to check if an item can be modified.</param>
    /// <param name="validProgressionSpecification">The specification to validate progressions.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AddTodoItemCommandHandler(
        ITodoListRepository repository, 
        IUnitOfWork unitOfWork,
        ICategoryValidator categoryValidator,
        CanModifyTodoItemSpecification canModifySpecification,
        ValidProgressionSpecification validProgressionSpecification)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _categoryValidator = categoryValidator ?? throw new ArgumentNullException(nameof(categoryValidator));
        _canModifySpecification = canModifySpecification ?? throw new ArgumentNullException(nameof(canModifySpecification));
        _validProgressionSpecification = validProgressionSpecification ?? throw new ArgumentNullException(nameof(validProgressionSpecification));
    }

    /// <summary>
    /// Handles the add todo item command.
    /// </summary>
    /// <param name="command">The command containing item details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result> Handle(AddTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = new TodoList(_categoryValidator, _canModifySpecification, _validProgressionSpecification);
            var newItem = aggregate.CreateValidatedItem(0, command.Title, command.Description, command.Category);
            
            await _repository.SaveAsync(newItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (InvalidCategoryException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
