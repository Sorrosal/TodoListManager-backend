// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to add a new todo item.
/// </summary>
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="AddTodoItemCommandHandler"/>.
    /// </summary>
    /// <param name="repository">The todo list repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <exception cref="ArgumentNullException">Thrown when repository or unitOfWork is null.</exception>
    public AddTodoItemCommandHandler(ITodoListRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            // Use 0 as temporary ID - EF will generate the real one
            var aggregate = await _repository.GetAggregateAsync(cancellationToken);

            aggregate.AddItem(0, command.Title, command.Description, command.Category);

            // Get the newly added item (it will have Id = 0 before saving)
            var newItem = aggregate.GetAllItems().First(i => i.Id == 0);
            
            await _repository.SaveAsync(newItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
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
