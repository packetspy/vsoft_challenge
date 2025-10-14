namespace TaskManagement.Core.Entities;

public class UserNotification
{
    public Guid PublicId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public Guid TaskItemId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}