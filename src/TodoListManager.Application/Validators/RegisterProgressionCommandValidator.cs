// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using TodoListManager.Application.Commands;

namespace TodoListManager.Application.Validators;

public class RegisterProgressionCommandValidator : AbstractValidator<RegisterProgressionCommand>
{
    public RegisterProgressionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Date cannot be in the future");

        RuleFor(x => x.Percent)
            .InclusiveBetween(0, 100).WithMessage("Percent must be between 0 and 100");
    }
}
