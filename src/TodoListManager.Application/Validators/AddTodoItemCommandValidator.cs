// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using TodoListManager.Application.Commands;

namespace TodoListManager.Application.Validators;

public class AddTodoItemCommandValidator : AbstractValidator<AddTodoItemCommand>
{
    public AddTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(BeValidCategory).WithMessage("Category must be 'Learning', 'Work', or 'Personal'");
    }

    private bool BeValidCategory(string category)
    {
        var validCategories = new[] { "Learning", "Work", "Personal" };
        return validCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }
}
