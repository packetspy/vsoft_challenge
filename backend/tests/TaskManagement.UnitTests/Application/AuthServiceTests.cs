using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.UnitTests.Application;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IPasswordService> _passwordService;
    private readonly Mock<IUserService> _userService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Use a unique database name for isolation
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
        _context = new ApplicationDbContext(options);

        _passwordService = new Mock<IPasswordService>();
        _passwordService.Setup(x => x.HashPassword("passwordFake")).Returns("hashedpassword");

        _configurationMock = new Mock<IConfiguration>();
        _passwordService = new Mock<IPasswordService>();

        // Mock JWT configuration
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(x => x["Key"]).Returns("SuperSecretKeyForJwtTokenGeneration12345");
        jwtSectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
        jwtSectionMock.Setup(x => x["Audience"]).Returns("TestAudience");

        _configurationMock.Setup(x => x.GetSection("Jwt")).Returns(jwtSectionMock.Object);
        _authService = new AuthService(_context, _configurationMock.Object, _passwordService.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@email.com",
            PasswordHash = "validpassword",
            Roles = new List<UserRole>
            {
                new UserRole { Name = "User", Permissions = new List<string> { "CanCreateTask" } }
            }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest { Username = "testuser", Password = "validpassword" };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.Equal("testuser", result.User.Username);
        Assert.Contains("CanCreateTask", result.User.Permissions);
    }

    // ...repeat for other tests, using _context.Users.Add(...) and _context.SaveChangesAsync() for setup

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}