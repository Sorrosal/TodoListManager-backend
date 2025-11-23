// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.Commands;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the command to register a progression for a todo item.
/// </summary>
public sealed class RegisterProgressionCommandHandler : IRequestHandler<RegisterProgressionCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITodoListFactory _todoListFactory;

    public RegisterProgressionCommandHandler(
        ITodoListRepository repository,
        IUnitOfWork unitOfWork,
        ITodoListFactory todoListFactory)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _todoListFactory = todoListFactory ?? throw new ArgumentNullException(nameof(todoListFactory));
    }

    /// <summary>
    /// Handles the register progression command.
    /// </summary>
    public async Task<Result> Handle(RegisterProgressionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (item == null) return Result.Failure($"Item with id {command.Id} not found");

            var aggregate = _todoListFactory.Create();
            aggregate.ValidateAndRegisterProgression(item, command.Date, command.Percent);

            await _repository.SaveAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (TodoItemNotFoundException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (InvalidProgressionException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
