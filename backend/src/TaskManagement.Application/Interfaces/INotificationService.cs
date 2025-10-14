using TaskManagement.Application.DTOs;
using TaskManagement.Core.Entities;

namespace TaskManagement.Application.Interfaces;

public interface INotificationService
{
    Task HandleNotificationAsync(NotificationMessage message);
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
}
