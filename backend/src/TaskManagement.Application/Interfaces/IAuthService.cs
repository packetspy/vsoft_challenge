using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<bool> ValidateUserAsync(string username, string password);
    Task<UserDto?> GetUserByUsernameAsync(string username);
}