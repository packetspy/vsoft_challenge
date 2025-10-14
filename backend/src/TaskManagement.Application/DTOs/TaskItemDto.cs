using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;

namespace TaskManagement.Application.DTOs;

public class TaskItemDto
{
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaskItemStatus Status { get; set; }
    public int Order { get; set; }
    public Guid AssignedToId { get; set; }
    public string? AssignedToUsername { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Guid? AssignedToId { get; set; } // If null, assign to current user
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskItemStatus? Status { get; set; }
    public Guid? AssignedToId { get; set; }
}

public class TaskReorderRequest
{
    public List<TaskReorderItem> Updates { get; set; } = new();
}

public class TaskReorderItem
{
    public Guid TaskId { get; set; }
    public TaskItemStatus Status { get; set; }
    public int Order { get; set; }
}

