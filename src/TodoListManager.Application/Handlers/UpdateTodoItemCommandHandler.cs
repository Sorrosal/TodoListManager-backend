// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to update an existing todo item.
/// </summary>
public sealed class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITodoListFactory _todoListFactory;

    public UpdateTodoItemCommandHandler(
        ITodoListRepository repository,
        IUnitOfWork unitOfWork,
        ITodoListFactory todoListFactory)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _todoListFactory = todoListFactory ?? throw new ArgumentNullException(nameof(todoListFactory));
    }

    /// <summary>
    /// Handles the update todo item command.
    /// </summary>
    /// <param name="command">The command containing the item ID and new description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result> Handle(UpdateTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (item == null) return Result.Failure($"Item with id {command.Id} not found");

            var aggregate = _todoListFactory.Create();
            aggregate.ValidateAndUpdateItem(item, command.Description);

            await _repository.SaveAsync(item, cancellationToken);
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
