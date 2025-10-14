using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagement.Api.Controllers;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Core.Entities;

namespace TaskManagement.UnitTests.Api;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _taskServiceMock = new Mock<ITaskService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _controller = new UsersController(_userServiceMock.Object, _currentUserServiceMock.Object);

        // Setup default user context
        SetupUserContext();
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersList()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { PublicId = Guid.NewGuid(), Username = "user1", Email = "user1@test.com" },
            new UserDto { PublicId = Guid.NewGuid(), Username = "user2", Email = "user2@test.com" }
        };

        _userServiceMock.Setup(service => service.GetUsersAsync())
                       .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
        Assert.Equal(2, returnedUsers.Count());
    }

    [Fact]
    public async Task CreateRandomUsers_ValidRequest_ReturnsSuccessMessage()
    {
        // Arrange
        var request = new CreateRandomUsersRequest
        {
            Amount = 5,
            UserNameMask = "testuser_{{random}}"
        };

        var createdUsers = new List<User>
        {
            new User { PublicId = Guid.NewGuid(), Username = "testuser_123", Email = "testuser_123@vsoft.tech" },
            new User { PublicId = Guid.NewGuid(), Username = "testuser_456", Email = "testuser_456@vsoft.tech" }
        };

        _userServiceMock.Setup(service => service.CreateRandomUsersAsync(request))
                       .ReturnsAsync(createdUsers);

        // Act
        var result = await _controller.CreateRandomUsers(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value.GetType();

        // Verify the service was called
        _userServiceMock.Verify(service => service.CreateRandomUsersAsync(request), Times.Once);
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