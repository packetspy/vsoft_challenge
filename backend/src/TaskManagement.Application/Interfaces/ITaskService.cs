using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<TaskItemDto> CreateTaskAsync(CreateTaskRequest request, Guid currentUserId);
    Task<IEnumerable<TaskItemDto>> GetTasksAsync(Guid currentUserId);
    Task<TaskItemDto?> GetTaskByIdAsync(Guid taskItemId, Guid currentUserId);
    Task<TaskItemDto?> UpdateTaskAsync(Guid taskItemId, UpdateTaskRequest request, Guid currentUserId);
    Task<bool> DeleteTaskAsync(Guid taskItemId, Guid currentUserId);
    Task ReorderTasksAsync(TaskReorderRequest request, Guid currentUserId);
}