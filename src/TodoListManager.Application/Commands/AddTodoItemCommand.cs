// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Application.Commands;

public record AddTodoItemCommand(string Title, string Description, string Category);
