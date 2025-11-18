// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Domain.Repositories;

public interface ITodoListRepository
{
    int GetNextId();
    List<string> GetAllCategories();
}
