using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Middleware;
using TaskManagement.Application.Constants;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ICurrentUserService _currentUserService;

    public TasksController(ITaskService taskService, ICurrentUserService currentUserService)
    {
        _taskService = taskService;
        _currentUserService = currentUserService;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var tasks = await _taskService.GetTasksAsync(currentUserId);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetTask(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var task = await _taskService.GetTaskByIdAsync(id, currentUserId);

        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    [RequirePermission(Permissions.CanCreateTask)]
    public async Task<ActionResult<TaskItemDto>> CreateTask(CreateTaskRequest request)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var task = await _taskService.CreateTaskAsync(request, currentUserId);
        return Ok(task);
    }


    [HttpPut("{id}")]
    [RequirePermission(Permissions.CanUpdateTask)]
    public async Task<ActionResult<TaskItemDto>> UpdateTask(Guid id, UpdateTaskRequest request)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var task = await _taskService.UpdateTaskAsync(id, request, currentUserId);

        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permissions.CanDeleteTask)]
    public async Task<ActionResult> DeleteTask(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var result = await _taskService.DeleteTaskAsync(id, currentUserId);

        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("reorder")]
    [Authorize]
    public async Task<ActionResult> ReorderTask(TaskReorderRequest request)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        await _taskService.ReorderTasksAsync(request, currentUserId);
        return Ok(new { message = "Tasks reordered successfully" });
    }
}