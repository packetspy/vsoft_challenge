using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.UnitTests.Application;

public class TaskServiceTests : IDisposable
{
    private readonly Mock<IMessageProducer> _messageProducerMock;
    private readonly ApplicationDbContext _context;
    private readonly TaskService _taskService;
    private readonly string _databaseName;

    public TaskServiceTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        _context = new ApplicationDbContext(options);
        _messageProducerMock = new Mock<IMessageProducer>();
        _taskService = new TaskService(_context, _messageProducerMock.Object);
    }

    private ApplicationDbContext CreateFreshContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetTaskByIdAsync_TaskNotFound_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();
        var service = new TaskService(freshContext, _messageProducerMock.Object);

        // Act
        var result = await service.GetTaskByIdAsync(taskId, currentUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTaskByIdAsync_UserNotAuthorized_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();

        var task = new TaskItem
        {
            PublicId = taskId,
            Title = "Other User Task",
            Description = "Other Description",
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedToId = otherUserId,
            CreatedById = otherUserId,
            Order = 0,
            Status = TaskItemStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await freshContext.Tasks.AddAsync(task);
        await freshContext.SaveChangesAsync();

        var service = new TaskService(freshContext, _messageProducerMock.Object);

        // Act
        var result = await service.GetTaskByIdAsync(taskId, currentUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTaskAsync_ValidRequest_CreatesTaskAndSendsNotification()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();
        var service = new TaskService(freshContext, _messageProducerMock.Object);

        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedToId = assignedUserId
        };

        // Act
        var result = await service.CreateTaskAsync(request, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Task", result.Title);
        Assert.Equal("Description", result.Description);
        Assert.Equal(assignedUserId, result.AssignedToId);
        Assert.Equal(currentUserId, result.CreatedById);
        Assert.Equal(TaskItemStatus.Pending, result.Status);

        var savedTask = await freshContext.Tasks.FirstOrDefaultAsync(t => t.PublicId == result.PublicId);
        Assert.NotNull(savedTask);
        Assert.Equal("New Task", savedTask.Title);

        _messageProducerMock.Verify(m => m.PublishTaskAssignedMessageAsync(
            It.Is<Guid>(id => id == result.PublicId),
            It.Is<Guid>(id => id == assignedUserId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_NoAssignedToId_AssignsToCurrentUser()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();
        var service = new TaskService(freshContext, _messageProducerMock.Object);

        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Description",
            DueDate = DateTime.UtcNow.AddDays(7)
            // AssignedToId é null
        };

        // Act
        var result = await service.CreateTaskAsync(request, currentUserId);

        // Assert
        Assert.Equal(currentUserId, result.AssignedToId);
        _messageProducerMock.Verify(m => m.PublishTaskAssignedMessageAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateTaskAsync_TaskNotFound_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();
        var service = new TaskService(freshContext, _messageProducerMock.Object);

        var request = new UpdateTaskRequest { Title = "Updated Title" };

        // Act
        var result = await service.UpdateTaskAsync(taskId, request, currentUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTaskAsync_UserNotOwner_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();

        var task = new TaskItem
        {
            PublicId = taskId,
            Title = "Other User Task",
            Description = "Other Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedById = otherUserId,
            AssignedToId = otherUserId,
            Status = TaskItemStatus.Pending,
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await freshContext.Tasks.AddAsync(task);
        await freshContext.SaveChangesAsync();

        var service = new TaskService(freshContext, _messageProducerMock.Object);

        var request = new UpdateTaskRequest { Title = "Updated Title" };

        // Act
        var result = await service.UpdateTaskAsync(taskId, request, currentUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTaskAsync_ValidTask_DeletesTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();

        var task = new TaskItem
        {
            PublicId = taskId,
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedById = currentUserId,
            Status = TaskItemStatus.Pending,
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await freshContext.Tasks.AddAsync(task);
        await freshContext.SaveChangesAsync();

        var service = new TaskService(freshContext, _messageProducerMock.Object);

        // Act
        var result = await service.DeleteTaskAsync(taskId, currentUserId);

        // Assert
        Assert.True(result);
        var deletedTask = await freshContext.Tasks.FindAsync(taskId);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task DeleteTaskAsync_TaskNotFound_ReturnsFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();
        var service = new TaskService(freshContext, _messageProducerMock.Object);

        // Act
        var result = await service.DeleteTaskAsync(taskId, currentUserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteTaskAsync_UserNotOwner_ReturnsFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        using var freshContext = CreateFreshContext();

        var task = new TaskItem
        {
            PublicId = taskId,
            Title = "Other User Task",
            Description = "Other Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedById = otherUserId,
            Status = TaskItemStatus.Pending,
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await freshContext.Tasks.AddAsync(task);
        await freshContext.SaveChangesAsync();

        var service = new TaskService(freshContext, _messageProducerMock.Object);

        // Act
        var result = await service.DeleteTaskAsync(taskId, currentUserId);

        // Assert
        Assert.False(result);
        var existingTask = await freshContext.Tasks.FindAsync(taskId);
        Assert.NotNull(existingTask);
    }

    [Fact]
    public async Task ReorderTasksAsync_ValidRequest_ReordersTasks()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var task1Id = Guid.NewGuid();
        var task2Id = Guid.NewGuid();

        using var freshContext = CreateFreshContext();

        var tasks = new List<TaskItem>
        {
            new TaskItem {
                PublicId = task1Id,
                Title = "Task 1",
                Description = "Description 1",
                DueDate = DateTime.UtcNow.AddDays(1),
                Status = TaskItemStatus.Pending,
                Order = 0,
                CreatedById = currentUserId,
                AssignedToId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem {
                PublicId = task2Id,
                Title = "Task 2",
                Description = "Description 2",
                DueDate = DateTime.UtcNow.AddDays(2),
                Status = TaskItemStatus.Pending,
                Order = 1,
                CreatedById = currentUserId,
                AssignedToId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await freshContext.Tasks.AddRangeAsync(tasks);
        await freshContext.SaveChangesAsync();

        var service = new TaskService(freshContext, _messageProducerMock.Object);

        var request = new TaskReorderRequest
        {
            Updates = new List<TaskReorderItem>
            {
                new TaskReorderItem { TaskId = task1Id, Status = TaskItemStatus.InProgress, Order = 0 },
                new TaskReorderItem { TaskId = task2Id, Status = TaskItemStatus.InProgress, Order = 1 }
            }
        };

        // Act
        await service.ReorderTasksAsync(request, currentUserId);

        // Assert
        var updatedTasks = await freshContext.Tasks.Where(t => t.CreatedById == currentUserId).ToListAsync();

        Assert.Equal(2, updatedTasks.Count);
        Assert.All(updatedTasks, t => Assert.Equal(TaskItemStatus.InProgress, t.Status));

        var task1 = updatedTasks.First(t => t.PublicId == task1Id);
        var task2 = updatedTasks.First(t => t.PublicId == task2Id);

        Assert.Equal(0, task1.Order);
        Assert.Equal(1, task2.Order);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}