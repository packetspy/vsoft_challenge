using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IMessageProducer _messageProducer;

    public TaskService(ApplicationDbContext context, IMessageProducer messageProducer)
    {
        _context = context;
        _messageProducer = messageProducer;
    }

    public async Task<IEnumerable<TaskItemDto>> GetTasksAsync(Guid currentUserId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Where(t => t.CreatedById == currentUserId || t.AssignedToId == currentUserId)
            .OrderBy(t => t.Order)
            .ToListAsync();

        return tasks.Select(MapToTaskDto);
    }

    public async Task<TaskItemDto?> GetTaskByIdAsync(Guid taskItemId, Guid currentUserId)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.PublicId == taskItemId &&
                                     (t.CreatedById == currentUserId || t.AssignedToId == currentUserId));

        return task != null ? MapToTaskDto(task) : null;
    }

    public async Task<TaskItemDto> CreateTaskAsync(CreateTaskRequest request, Guid currentUserId)
    {
        try
        {
            var assignedToId = request.AssignedToId ?? currentUserId;
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Status = Core.Enums.TaskItemStatus.Pending,
                AssignedToId = assignedToId,
                CreatedById = currentUserId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Notify assigned user if it's not the current user
            if (assignedToId != currentUserId)
                await _messageProducer.PublishTaskAssignedMessageAsync(task.PublicId, assignedToId);

            return MapToTaskDto(task);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<TaskItemDto?> UpdateTaskAsync(Guid taskItemId, UpdateTaskRequest request, Guid currentUserId)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.PublicId == taskItemId && t.CreatedById == currentUserId);

        if (task == null) return null;

        var originalAssignedToId = task.AssignedToId;

        if (!string.IsNullOrEmpty(request.Title))
            task.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            task.Description = request.Description;

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate.Value;

        if (request.Status.HasValue)
            task.Status = request.Status.Value;

        if (request.AssignedToId.HasValue)
            task.AssignedToId = request.AssignedToId.Value;

        await _context.SaveChangesAsync();

        // Notify if assignment changed to a different user
        if (request.AssignedToId.HasValue &&
            request.AssignedToId.Value != originalAssignedToId &&
            request.AssignedToId.Value != currentUserId)
        {
            await _messageProducer.PublishTaskAssignedMessageAsync(task.PublicId, request.AssignedToId.Value);
        }

        return MapToTaskDto(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid taskItemId, Guid currentUserId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.PublicId == taskItemId && t.CreatedById == currentUserId);

        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ReorderTasksAsync(TaskReorderRequest request, Guid currentUserId)
    {
        try
        {
            // Group updates by status
            var updatesByStatus = request.Updates
                .GroupBy(u => u.Status)
                .ToDictionary(g => g.Key, g => g.OrderBy(u => u.Order).ToList());

            foreach (var (status, statusUpdates) in updatesByStatus)
            {
                // Get all user tasks with this status (to reorder everything)
                var userTaskIdsInStatus = statusUpdates.Select(u => u.TaskId).ToList();
                var allTasksInStatus = await _context.Tasks
                    .Where(t => (t.CreatedById == currentUserId || t.AssignedToId == currentUserId) &&
                               t.Status == status)
                    .ToListAsync();

                // Reorder all tasks with this status
                // First, reset the order of all tasks to high values
                foreach (var task in allTasksInStatus)
                    task.Order = task.Order + 1000;

                await _context.SaveChangesAsync();

                // Now assign the new orders sequentially
                var taskOrderMap = statusUpdates
                    .Select((update, index) => new { update.TaskId, NewOrder = index })
                    .ToDictionary(x => x.TaskId, x => x.NewOrder);

                // Update tasks that have been moved/reordered
                foreach (var update in statusUpdates)
                {
                    var task = new TaskItem();
                    if (allTasksInStatus.Count > 0)
                        task = allTasksInStatus.First(t => t.PublicId == update.TaskId);
                    else
                        task = await _context.Tasks.FirstAsync(t => t.PublicId == update.TaskId);
                
                    task.Order = taskOrderMap[task.PublicId];
                    task.Status = status;
                    task.UpdatedAt = DateTime.UtcNow;
                }

                // For tasks that were not included in the reorder (keep them in their original relative order)
                var remainingTasks = allTasksInStatus
                    .Where(t => !userTaskIdsInStatus.Contains(t.PublicId))
                    .OrderBy(t => t.Order)
                    .ToList();

                for (int i = 0; i < remainingTasks.Count; i++)
                    remainingTasks[i].Order = statusUpdates.Count + i;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private TaskItemDto MapToTaskDto(TaskItem taskItem)
    {
        return new TaskItemDto
        {
            PublicId = taskItem.PublicId,
            Title = taskItem.Title,
            Description = taskItem.Description,
            DueDate = taskItem.DueDate,
            Status = taskItem.Status,
            Order = taskItem.Order,
            AssignedToId = taskItem.AssignedToId,
            AssignedToUsername = taskItem.AssignedTo?.Username,
            CreatedById = taskItem.CreatedById,
            CreatedAt = taskItem.CreatedAt,
            UpdatedAt = taskItem.UpdatedAt
        };
    }
}