// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using MediatR;
using TodoListManager.Domain.Common;

namespace TodoListManager.Application.Commands;

public record AddTodoItemCommand(string Title, string Description, string Category) : IRequest<Result>;
