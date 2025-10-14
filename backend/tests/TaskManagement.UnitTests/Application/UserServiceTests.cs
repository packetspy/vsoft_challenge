using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.UnitTests.Application;

public class UserServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly Mock<IPasswordService> _passwordService;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _passwordService = new Mock<IPasswordService>();
        _passwordService.Setup(x => x.HashPassword("passwordFake")).Returns("hashedpassword");

        _userService = new UserService(_context, _passwordService.Object);
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { PublicId = Guid.NewGuid(), Username = "user1", Email = "user1@email.com" },
            new User { PublicId = Guid.NewGuid(), Username = "user2", Email = "user2@email.com" }
        };
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateRandomUsersAsync_ValidRequest_CreatesUsers()
    {
        // Arrange
        var request = new CreateRandomUsersRequest
        {
            Amount = 5,
            UserNameMask = "testuser_{{random}}"
        };

        // Act
        var result = await _userService.CreateRandomUsersAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        Assert.Equal(5, _context.Users.Count());
    }

    [Fact]
    public async Task GetUserByIdAsync_ValidUserId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { PublicId = userId, Username = "testuser", Email = "test@email.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ValidUsername_ReturnsUser()
    {
        // Arrange
        var user = new User { PublicId = Guid.NewGuid(), Username = "testuser", Email = "test@email.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}