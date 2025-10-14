using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(
        INotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetUserNotifications()
    {
        var userId = _currentUserService.GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var userId = _currentUserService.GetCurrentUserId();
        await _notificationService.MarkAsReadAsync(notificationId, userId);
        return Ok();
    }

    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(Guid notificationId)
    {
        var userId = _currentUserService.GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok();
    }
}