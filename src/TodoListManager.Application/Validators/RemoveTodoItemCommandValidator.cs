// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using TodoListManager.Application.Commands;

namespace TodoListManager.Application.Validators;

public class RemoveTodoItemCommandValidator : AbstractValidator<RemoveTodoItemCommand>
{
    public RemoveTodoItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");
    }
}
