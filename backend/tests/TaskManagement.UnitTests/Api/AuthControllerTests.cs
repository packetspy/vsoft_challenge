using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.Api.Controllers;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.UnitTests.Api;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        var authResponse = new AuthResponse
        {
            Token = "jwt-token-here",
            User = new UserDto
            {
                PublicId = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@email.com",
                Permissions = new List<string> { "CanCreateTask" }
            }
        };

        _authServiceMock.Setup(service => service.LoginAsync(request))
                       .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAuth = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("jwt-token-here", returnedAuth.Token);
        Assert.Equal("testuser", returnedAuth.User.Username);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _authServiceMock.Setup(service => service.LoginAsync(request))
                       .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Invalid username or password", unauthorizedResult.Value);
    }

    [Fact]
    public async Task ValidateUser_ValidCredentials_ReturnsTrue()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        _authServiceMock.Setup(service => service.ValidateUserAsync(request.Username, request.Password))
                       .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateUser(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var isValid = Assert.IsType<bool>(okResult.Value);
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateUser_InvalidCredentials_ReturnsFalse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _authServiceMock.Setup(service => service.ValidateUserAsync(request.Username, request.Password))
                       .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateUser(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var isValid = Assert.IsType<bool>(okResult.Value);
        Assert.False(isValid);
    }
}