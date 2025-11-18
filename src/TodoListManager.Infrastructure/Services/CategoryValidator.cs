// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;

namespace TodoListManager.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of category validation using repository.
/// </summary>
public class CategoryValidator : ICategoryValidator
{
    private readonly ITodoListRepository _repository;

    public CategoryValidator(ITodoListRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public bool IsValidCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;

        var validCategories = _repository.GetAllCategories();
        return validCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> GetValidCategories()
    {
        return _repository.GetAllCategories().ToList().AsReadOnly();
    }
}
