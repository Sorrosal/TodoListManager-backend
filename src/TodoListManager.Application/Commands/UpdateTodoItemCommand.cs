// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.Commands;

public record UpdateTodoItemCommand(int Id, string Description);
