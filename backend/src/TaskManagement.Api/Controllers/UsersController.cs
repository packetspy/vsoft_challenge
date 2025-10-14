using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var tasks = await _userService.GetUsersAsync();
        return Ok(tasks);
    }

    [HttpPost("createRandom")]
    public async Task<ActionResult> CreateRandomUsers(CreateRandomUsersRequest request)
    {
        var users = await _userService.CreateRandomUsersAsync(request);
        return Ok(
            new { 
                message = $"{request.Amount} random users created successfully", 
                defaultPassword = "default123",
                users 
            });
    }

    [HttpPost("register")]
    public async Task<ActionResult> CreateNewUser(RegisterRequest request)
    {
        var user = await _userService.CreateSingleUser(request);
        return Ok(new {message = $"User created successfully" });
    }
}