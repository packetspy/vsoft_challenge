namespace TaskManagement.Core.Entities;

public class NotificationMessage
{
    public Guid MessageId { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}