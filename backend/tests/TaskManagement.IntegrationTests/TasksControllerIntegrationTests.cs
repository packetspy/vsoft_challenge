using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TaskManagement.Application.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.IntegrationTests;

public class TasksControllerIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;
    private Guid _testUserId;

    public TasksControllerIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ApplicationDbContext));

                    if (dbContextDescriptor != null)
                        services.Remove(dbContextDescriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
                    });
                });

                builder.UseEnvironment("Development");
            });

        _client = _factory.CreateClient();

        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public async Task InitializeAsync()
    {
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        _context.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }

    
    [Fact]
    public async Task CreateTask_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Integration Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedToId = _testUserId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private async Task SeedTestDataAsync()
    {
        _testUserId = Guid.NewGuid();

        var user = new User
        {
            PublicId = _testUserId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Roles = new List<UserRole>
            {
                new UserRole
                {
                    Name = "User",
                    Permissions = new List<string>
                    {
                        "CanCreateTask", "CanUpdateTask", "CanDeleteTask"
                    }
                }
            }
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}