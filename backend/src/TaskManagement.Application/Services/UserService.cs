using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;

    public UserService(ApplicationDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<IEnumerable<UserDto>?> GetUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapToUserDto);
    }

    public async Task<IEnumerable<User>> CreateRandomUsersAsync(CreateRandomUsersRequest request)
    {
        var existingUsernames = await _context.Users
            .Select(u => u.Username)
            .ToListAsync();

        var existingUsernamesSet = existingUsernames.ToHashSet();

        var random = new Random();
        var usersToCreate = new List<User>();

        for (int i = 0; i < request.Amount; i++)
        {
            string username;
            int attempts = 0;

            do
            {
                var randomPart = random.Next(1000, 999999).ToString();
                username = request.UserNameMask.Replace("{{random}}", randomPart);
                attempts++;
            } while (existingUsernamesSet.Contains(username) && attempts < 10);

            if (attempts >= 10)
                username = request.UserNameMask.Replace("{{random}}", $"{random.Next(1000, 9999)}_{DateTime.Now.Ticks}"); // Fallback

            var user = new User
            {
                Username = username,
                Email = $"{username}@vsoft.tech",
                PasswordHash = _passwordService.HashPassword(),
                Roles =
                [
                    new() {
                        Name = "User",
                        Permissions = [
                            "CanCreateTask",
                            "CanUpdateTask",
                            "CanDeleteTask"
                        ]
                    }
                ]
            };

            usersToCreate.Add(user);
            existingUsernamesSet.Add(username);

            // Batch save every 100 users to avoid memory issues
            if (usersToCreate.Count >= 100)
            {
                _context.Users.AddRange(usersToCreate);
                await _context.SaveChangesAsync();
                usersToCreate.Clear();
            }
        }

        // Save any remaining users
        if (usersToCreate.Any())
        {
            _context.Users.AddRange(usersToCreate);
            await _context.SaveChangesAsync();
        }

        return usersToCreate;
    }

    public async Task<UserDto> CreateSingleUser(RegisterRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (existingUser != null)
            return MapToUserDto(existingUser);

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Roles =
                [
                    new() {
                        Name = "User",
                        Permissions = [
                            "CanCreateTask",
                            "CanUpdateTask",
                            "CanDeleteTask"
                        ]
                    }
                ]
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return MapToUserDto(newUser);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PublicId == userId);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            PublicId = user.PublicId,
            Username = user.Username,
            Email = user.Email,
            Permissions = user.Roles.SelectMany(r => r.Permissions).Distinct().ToList()
        };
    }
}