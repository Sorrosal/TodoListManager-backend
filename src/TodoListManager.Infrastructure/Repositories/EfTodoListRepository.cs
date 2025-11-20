using Microsoft.EntityFrameworkCore;
using TodoListManager.Domain.Repositories;
using TodoListManager.Infrastructure.Persistence.Entities;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Services;
using TodoListManager.Domain.Entities;
using TodoListManager.Infrastructure.Persistence;

namespace TodoListManager.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of ITodoListRepository.
/// Infrastructure concern - bridges domain to EF Core.
/// Follows Repository pattern and Clean Architecture principles.
/// </summary>
public class EfTodoListRepository : ITodoListRepository
{
    private readonly TodoDbContext _db;
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

    public EfTodoListRepository(TodoDbContext db, ICategoryValidator categoryValidator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _categoryValidator = categoryValidator ?? throw new ArgumentNullException(nameof(categoryValidator));
    }

    public List<string> GetAllCategories()
    {
        return ValidCategories.ToList();
    }

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.TodoItems
            .Include(t => t.Progressions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<List<TodoItem>> GetAllDomainItemsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.TodoItems
            .Include(t => t.Progressions)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task SaveAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var existing = await _db.TodoItems
            .Include(t => t.Progressions)
            .FirstOrDefaultAsync(t => t.Id == item.Id, cancellationToken);

        if (existing == null)
        {
            var newEntity = MapToEntity(item);
            await _db.TodoItems.AddAsync(newEntity, cancellationToken);
        }
        else
        {
            UpdateEntity(existing, item);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var existing = await _db.TodoItems
            .Include(t => t.Progressions)
            .FirstOrDefaultAsync(t => t.Id == item.Id, cancellationToken);

        if (existing != null)
        {
            _db.TodoItems.Remove(existing);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<TodoList> GetAggregateAsync(CancellationToken cancellationToken = default)
    {
        var list = new TodoList(_categoryValidator);
        var items = await _db.TodoItems
            .Include(t => t.Progressions)
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);

        foreach (var entity in items)
        {
            var item = MapToDomain(entity);
            list.AddItem(item.Id, item.Title, item.Description, item.Category);
            
            foreach (var progression in item.Progressions)
            {
                list.RegisterProgression(item.Id, progression.Date, progression.Percent);
            }
        }

        return list;
    }

    /// <summary>
    /// Maps EF entity to domain entity.
    /// </summary>
    private TodoItem MapToDomain(TodoItemEntity entity)
    {
        var item = new TodoItem(
            entity.Id, 
            entity.Title, 
            entity.Description ?? string.Empty, 
            entity.Category);

        foreach (var progression in entity.Progressions.OrderBy(p => p.Date))
        {
            item.AddProgression(progression.Date, progression.Percent);
        }

        return item;
    }

    /// <summary>
    /// Maps domain entity to EF entity.
    /// </summary>
    private TodoItemEntity MapToEntity(TodoItem item)
    {
        return new TodoItemEntity
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Category = item.Category,
            Progressions = item.Progressions
                .Select(p => new ProgressionEntity 
                { 
                    Date = p.Date, 
                    Percent = p.Percent 
                })
                .ToList()
        };
    }

    /// <summary>
    /// Updates existing EF entity with domain entity data.
    /// </summary>
    private void UpdateEntity(TodoItemEntity existing, TodoItem item)
    {
        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.Category = item.Category;

        // Sync progressions: remove old, add new
        _db.Progressions.RemoveRange(existing.Progressions);
        existing.Progressions = item.Progressions
            .Select(p => new ProgressionEntity 
            { 
                TodoItemId = existing.Id, 
                Date = p.Date, 
                Percent = p.Percent 
            })
            .ToList();
    }
}
