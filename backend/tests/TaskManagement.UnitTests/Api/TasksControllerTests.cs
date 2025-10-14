using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagement.Api.Controllers;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Core.Enums;

namespace TaskManagement.UnitTests.Api;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _controller = new TasksController(_taskServiceMock.Object, _currentUserServiceMock.Object);

        // Setup default user context
        SetupUserContext();
    }

    [Fact]
    public async Task GetTasks_ValidUser_ReturnsTasks()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var tasks = new List<TaskItemDto>
        {
            new TaskItemDto { PublicId = Guid.NewGuid(), Title = "Task 1", Status = TaskItemStatus.Pending },
            new TaskItemDto { PublicId = Guid.NewGuid(), Title = "Task 2", Status = TaskItemStatus.InProgress }
        };

        SetupUserContext(currentUserId);
        _currentUserServiceMock.Setup(service => service.GetCurrentUserId())
                              .Returns(currentUserId);
        _taskServiceMock.Setup(service => service.GetTasksAsync(currentUserId))
                       .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetTasks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTasks = Assert.IsAssignableFrom<IEnumerable<TaskItemDto>>(okResult.Value);
        Assert.Equal(2, returnedTasks.Count());
    }

    [Fact]
    public async Task GetTask_ValidTaskAndUser_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var task = new TaskItemDto { PublicId = taskId, Title = "Test Task" };

        SetupUserContext(currentUserId);
        _currentUserServiceMock.Setup(service => service.GetCurrentUserId())
                              .Returns(currentUserId);
        _taskServiceMock.Setup(service => service.GetTaskByIdAsync(taskId, currentUserId))
                       .ReturnsAsync(task);

        // Act
        var result = await _controller.GetTask(taskId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTask = Assert.IsType<TaskItemDto>(okResult.Value);
        Assert.Equal(taskId, returnedTask.PublicId);
        Assert.Equal("Test Task", returnedTask.Title);
    }

    [Fact]
    public async Task GetTask_TaskNotFound_ReturnsNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        SetupUserContext(currentUserId);
        _taskServiceMock.Setup(service => service.GetTaskByIdAsync(taskId, currentUserId))
                       .ReturnsAsync((TaskItemDto?)null);

        // Act
        var result = await _controller.GetTask(taskId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateTask_ValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var createdTask = new TaskItemDto
        {
            PublicId = Guid.NewGuid(),
            Title = "New Task",
            CreatedById = currentUserId
        };

        SetupUserContext(currentUserId);
        _currentUserServiceMock.Setup(service => service.GetCurrentUserId())
                              .Returns(currentUserId); // Ensure the mock returns the correct user ID
        _taskServiceMock.Setup(service => service.CreateTaskAsync(request, currentUserId))
                       .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTask = Assert.IsType<TaskItemDto>(okResult.Value);
        Assert.Equal("New Task", returnedTask.Title);
        Assert.Equal(currentUserId, returnedTask.CreatedById);
    }

    [Fact]
    public async Task UpdateTask_ValidRequest_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var request = new UpdateTaskRequest { Title = "Updated Task" };
        var updatedTask = new TaskItemDto { PublicId = taskId, Title = "Updated Task" };

        SetupUserContext(currentUserId);
        _currentUserServiceMock.Setup(service => service.GetCurrentUserId())
                              .Returns(currentUserId); // Ensure the mock returns the correct user ID
        _taskServiceMock.Setup(service => service.UpdateTaskAsync(taskId, request, currentUserId))
                       .ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.UpdateTask(taskId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTask = Assert.IsType<TaskItemDto>(okResult.Value);
        Assert.Equal("Updated Task", returnedTask.Title);
    }

    [Fact]
    public async Task UpdateTask_TaskNotFound_ReturnsNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var request = new UpdateTaskRequest { Title = "Updated Task" };

        SetupUserContext(currentUserId);
        _taskServiceMock.Setup(service => service.UpdateTaskAsync(taskId, request, currentUserId))
                       .ReturnsAsync((TaskItemDto?)null);

        // Act
        var result = await _controller.UpdateTask(taskId, request);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteTask_ValidTask_ReturnsNoContent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        SetupUserContext(currentUserId);
        _currentUserServiceMock.Setup(service => service.GetCurrentUserId())
                              .Returns(currentUserId); // Ensure the mock returns the correct user ID
        _taskServiceMock.Setup(service => service.DeleteTaskAsync(taskId, currentUserId))
                       .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTask_TaskNotFound_ReturnsNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        SetupUserContext(currentUserId);
        _taskServiceMock.Setup(service => service.DeleteTaskAsync(taskId, currentUserId))
                       .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task ReorderTask_ValidRequest_ReturnsOk()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var request = new TaskReorderRequest
        {
            Updates = new List<TaskReorderItem>
            {
                new TaskReorderItem { TaskId = Guid.NewGuid(), Status = TaskItemStatus.InProgress, Order = 0 },
                new TaskReorderItem { TaskId = Guid.NewGuid(), Status = TaskItemStatus.InProgress, Order = 1 }
            }
        };

        SetupUserContext(currentUserId);
        _taskServiceMock.Setup(service => service.ReorderTasksAsync(request, currentUserId))
                       .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ReorderTask(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("Tasks reordered successfully", okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task GetCurrentUserId_InvalidUserClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var controller = new TasksController(_taskServiceMock.Object, _currentUserServiceMock.Object);

        // Set up the mock to throw UnauthorizedAccessException when GetCurrentUserId is called
        _currentUserServiceMock
            .Setup(service => service.GetCurrentUserId())
            .Throws<UnauthorizedAccessException>();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.GetTasks());
    }

    private void SetupUserContext(Guid? userId = null)
    {
        var actualUserId = userId ?? Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, actualUserId.ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }
}