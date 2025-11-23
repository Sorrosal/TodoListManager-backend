// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using AutoMapper;
using MediatR;
using TodoListManager.Application.DTOs;
using TodoListManager.Application.Queries;
using TodoListManager.Domain.Common;
using TodoListManager.Domain.Exceptions;
using TodoListManager.Domain.Repositories;

namespace TodoListManager.Application.Handlers;

/// <summary>
/// Handles the query to retrieve all todo items.
/// </summary>
public sealed class GetAllTodoItemsQueryHandler : IRequestHandler<GetAllTodoItemsQuery, Result<GetAllTodoItemsResponse>>
{
    private readonly ITodoListRepository _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="GetAllTodoItemsQueryHandler"/>.
    /// </summary>
    /// <param name="repository">The todo list repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when repository or mapper is null.</exception>
    public GetAllTodoItemsQueryHandler(ITodoListRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Handles the get all todo items query.
    /// </summary>
    /// <param name="query">The query instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the todo items response or an error.</returns>
    public async Task<Result<GetAllTodoItemsResponse>> Handle(GetAllTodoItemsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var itemsFromRepo = await _repository.GetAllDomainItemsAsync(cancellationToken);
            var itemDtos = _mapper.Map<List<TodoItemDto>>(itemsFromRepo);
            
            var response = new GetAllTodoItemsResponse
            {
                Items = itemDtos,
                TotalCount = itemDtos.Count
            };
            
            return Result.Success(response);
        }
        catch (DomainException ex)
        {
            return Result.Failure<GetAllTodoItemsResponse>(ex.Message);
        }
    }
}
