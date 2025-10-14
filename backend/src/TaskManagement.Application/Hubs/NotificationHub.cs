using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Application.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async Task OnConnectedAsync()
    {
        var userPublicId = _currentUserService.GetCurrentUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userPublicId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnDisconnectedAsync(exception);
    }

    public async Task MarkAsRead(string notificationId)
    {
        await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
    }
}