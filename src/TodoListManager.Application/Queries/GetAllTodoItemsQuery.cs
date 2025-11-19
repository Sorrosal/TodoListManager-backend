// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Application.DTOs;
using TodoListManager.Domain.Common;

namespace TodoListManager.Application.Queries;

public record GetAllTodoItemsQuery : IRequest<Result<GetAllTodoItemsResponse>>;
