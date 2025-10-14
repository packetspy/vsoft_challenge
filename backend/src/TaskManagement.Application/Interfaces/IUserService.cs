using TaskManagement.Application.DTOs;
using TaskManagement.Core.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>?> GetUsersAsync();
    Task<IEnumerable<User>> CreateRandomUsersAsync(CreateRandomUsersRequest request);
    Task<UserDto> CreateSingleUser(RegisterRequest request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
}
