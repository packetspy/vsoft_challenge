using System.Security.Claims;

namespace TaskManagement.Application.Interfaces;

public interface ICurrentUserService
{
    Guid GetCurrentUserId();
    string Username { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    ClaimsPrincipal User { get; }
}