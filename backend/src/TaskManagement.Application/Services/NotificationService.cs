using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Hubs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger, IHubContext<NotificationHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
        _context = context;
    }

    public async Task HandleNotificationAsync(NotificationMessage message)
    {
        try
        {
            var notification = new UserNotification
            {
                PublicId = Guid.NewGuid(),
                UserId = message.AssignedToUserId,
                Title = GetNotificationTitle(message.MessageType),
                Message = message.Description,
                TaskItemId = message.TaskItemId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                NotificationType = message.MessageType
            };

            _context.UserNotifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"user-{message.AssignedToUserId}")
                .SendAsync("ReceiveNotification", new
                {
                    Id = notification.PublicId,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    TaskItemId = notification.TaskItemId,
                    NotificationType = notification.NotificationType
                });

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId)
    {
        return await _context.UserNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto
            {
                Id = n.PublicId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TaskItemId = n.TaskItemId,
                NotificationType = n.NotificationType
            })
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.PublicId == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _context.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var notification in notifications)
            notification.IsRead = true;

        await _context.SaveChangesAsync();
    }

    private static string GetNotificationTitle(string messageType) => messageType switch
    {
        "TaskAssigned" => "New Task Assigned",
        "TaskUpdated" => "Updated Task",
        "TaskCompleted" => "Task Completed",
        _ => "New Notification"
    };
}