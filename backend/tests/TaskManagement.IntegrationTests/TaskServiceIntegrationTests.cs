using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.IntegrationTests;

public class TaskServiceIntegrationTests : IAsyncLifetime
{
    private ApplicationDbContext _context;
    private TaskService _taskService;
    private Mock<IMessageProducer> _messageProducerMock;

    public async Task InitializeAsync()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _messageProducerMock = new Mock<IMessageProducer>();
        _taskService = new TaskService(_context, _messageProducerMock.Object);

        // Seed test data
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        _context?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateTaskAsync_IntegrationTest_CreatesTaskInDatabase()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "Integration Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await _taskService.CreateTaskAsync(request, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Integration Test Task", result.Title);

        // Verify task was saved to database
        var savedTask = await _context.Tasks.FindAsync(result.PublicId);
        Assert.NotNull(savedTask);
        Assert.Equal("Integration Test Task", savedTask.Title);
    }

    [Fact]
    public async Task GetTasksAsync_IntegrationTest_ReturnsUserTasks()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        // Act
        var result = await _taskService.GetTasksAsync(currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, task =>
            Assert.True(task.CreatedById == currentUserId || task.AssignedToId == currentUserId));
    }

    private async Task SeedTestDataAsync()
    {
        var user1 = new User { PublicId = Guid.NewGuid(), Username = "user1", Email = "user1@test.com", PasswordHash = "pass1" };
        var user2 = new User { PublicId = Guid.NewGuid(), Username = "user2", Email = "user2@test.com", PasswordHash = "pass2" };

        var tasks = new List<TaskItem>
        {
            new TaskItem { PublicId = Guid.NewGuid(), Title = "Task 1", CreatedById = user1.PublicId, AssignedToId = user1.PublicId, Order = 0 },
            new TaskItem { PublicId = Guid.NewGuid(), Title = "Task 2", CreatedById = user1.PublicId, AssignedToId = user2.PublicId, Order = 1 },
            new TaskItem { PublicId = Guid.NewGuid(), Title = "Task 3", CreatedById = user2.PublicId, AssignedToId = user2.PublicId, Order = 0 }
        };

        _context.Users.AddRange(user1, user2);
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();
    }
}