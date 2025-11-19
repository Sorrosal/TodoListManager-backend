using TodoListManager.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListManager.Infrastructure.Persistence.Entities;

[Table("TodoItems")]
public class TodoItemEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public ICollection<ProgressionEntity> Progressions { get; set; } = new List<ProgressionEntity>();
}
