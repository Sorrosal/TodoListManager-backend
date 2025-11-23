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
/// Handles the command to remove a todo item.
/// </summary>
public sealed class RemoveTodoItemCommandHandler : IRequestHandler<RemoveTodoItemCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryValidator _categoryValidator;
    private readonly CanModifyTodoItemSpecification _canModifySpecification;
    private readonly ValidProgressionSpecification _validProgressionSpecification;

    /// <summary>
    /// Initializes a new instance of <see cref="RemoveTodoItemCommandHandler"/>.
    /// </summary>
    /// <param name="repository">The todo list repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="categoryValidator">The category validator service.</param>
    /// <param name="canModifySpecification">The specification to check if an item can be modified.</param>
    /// <param name="validProgressionSpecification">The specification to validate progressions.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public RemoveTodoItemCommandHandler(
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
    /// Handles the remove todo item command.
    /// </summary>
    /// <param name="command">The command containing the item ID to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result> Handle(RemoveTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (item == null) return Result.Failure($"Item with id {command.Id} not found");

            var aggregate = new TodoList(_categoryValidator, _canModifySpecification, _validProgressionSpecification);
            aggregate.ValidateCanRemoveItem(item);

            await _repository.DeleteAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (TodoItemNotFoundException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (TodoItemCannotBeModifiedException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
