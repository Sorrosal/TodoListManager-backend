using Microsoft.EntityFrameworkCore;
using TodoListManager.Domain.Repositories;
using TodoListManager.Infrastructure.Persistence.Entities;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Services;
using TodoListManager.Domain.Entities;
using TodoListManager.Infrastructure.Persistence;
using TodoListManager.Domain.Common;

namespace TodoListManager.Infrastructure.Repositories;

public class EfTodoListRepository : ITodoListRepository
{
    private readonly TodoDbContext _db;
    private readonly IGenericRepository<TodoItemEntity> _genericRepo;
    private readonly ICategoryValidator _categoryValidator;

    private static readonly string[] ValidCategories = new[]
    {
        "Work",
        "Personal",
        "Education",
        "Health",
        "Finance",
        "Other"
    };

    public EfTodoListRepository(TodoDbContext db, IGenericRepository<TodoItemEntity> genericRepo, ICategoryValidator categoryValidator)
    {
        _db = db;
        _genericRepo = genericRepo;
        _categoryValidator = categoryValidator;
    }

    public List<string> GetAllCategories()
    {
        return ValidCategories.ToList();
    }

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.TodoItems.Include(t => t.Progressions).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity == null) return null;
        return MapToDomain(entity);
    }

    public async Task<List<TodoItem>> GetAllDomainItemsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.TodoItems.Include(t => t.Progressions).ToListAsync(cancellationToken);
        return entities.Select(MapToDomain).ToList();
    }

    public async Task SaveAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        var existing = await _db.TodoItems.Include(t => t.Progressions).FirstOrDefaultAsync(t => t.Id == item.Id, cancellationToken);
        if (existing == null)
        {
            var newEntity = MapToEntity(item);
            await _genericRepo.AddAsync(newEntity, cancellationToken);
        }
        else
        {
            existing.Title = item.Title;
            existing.Description = item.Description;
            existing.Category = item.Category;

            // sync progressions: simple approach: clear and add
            _db.Progressions.RemoveRange(existing.Progressions);
            existing.Progressions = item.Progressions.Select(p => new ProgressionEntity { TodoItemId = existing.Id, Date = p.Date, Percent = p.Percent }).ToList();
            await _genericRepo.UpdateAsync(existing, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        var existing = await _db.TodoItems.Include(t => t.Progressions).FirstOrDefaultAsync(t => t.Id == item.Id, cancellationToken);
        if (existing == null) return;
        await _genericRepo.DeleteAsync(existing, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<TodoList> GetAggregateAsync(CancellationToken cancellationToken = default)
    {
        // Build a TodoList aggregate from all items
        var list = new TodoList(_categoryValidator);
        var items = _db.TodoItems.Include(t => t.Progressions).AsEnumerable().OrderBy(e => e.Id);
        foreach (var e in items)
        {
            var item = MapToDomain(e);
            // use internal AddItem to the aggregate
            list.AddItem(item.Id, item.Title, item.Description, item.Category);
            foreach (var p in item.Progressions)
            {
                list.RegisterProgression(item.Id, p.Date, p.Percent);
            }
        }

        return Task.FromResult(list);
    }

    private TodoItem MapToDomain(TodoItemEntity e)
    {
        var item = new TodoItem(e.Id, e.Title, e.Description ?? string.Empty, e.Category);
        foreach (var p in e.Progressions.OrderBy(p => p.Date)) item.AddProgression(p.Date, p.Percent);
        return item;
    }

    private TodoItemEntity MapToEntity(TodoItem item)
    {
        return new TodoItemEntity
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Category = item.Category,
            Progressions = item.Progressions.Select(p => new ProgressionEntity { Date = p.Date, Percent = p.Percent }).ToList()
        };
    }
}
