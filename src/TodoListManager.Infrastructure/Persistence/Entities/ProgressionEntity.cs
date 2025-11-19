using TodoListManager.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListManager.Infrastructure.Persistence.Entities;

[Table("Progressions")]
public class ProgressionEntity : BaseEntity
{
    public int TodoItemId { get; set; }
    public DateTime Date { get; set; }
    public decimal Percent { get; set; }

    public TodoItemEntity? TodoItem { get; set; }
}
