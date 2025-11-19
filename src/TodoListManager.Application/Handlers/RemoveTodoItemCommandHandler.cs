// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to remove a todo item.
/// </summary>
public class RemoveTodoItemCommandHandler : IRequestHandler<RemoveTodoItemCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="RemoveTodoItemCommandHandler"/>.
    /// </summary>
    /// <param name="repository">The todo list repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <exception cref="ArgumentNullException">Thrown when repository or unitOfWork is null.</exception>
    public RemoveTodoItemCommandHandler(ITodoListRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            var aggregate = await _repository.GetAggregateAsync(cancellationToken);
            aggregate.RemoveItem(command.Id);

            var item = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (item != null)
            {
                await _repository.DeleteAsync(item, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

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
