using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public int Order { get; set; } = 0;

    // Foreign keys
    public Guid AssignedToId { get; set; }
    public Guid CreatedById { get; set; }

    // Navigation properties
    public virtual User? AssignedTo { get; set; }
    public virtual User? CreatedBy { get; set; }
}
