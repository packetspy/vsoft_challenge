namespace TaskManagement.Application.Interfaces;

public interface IMessageProducer
{
    Task PublishTaskAssignedMessageAsync(Guid taskItemId, Guid assignedToUserId);
    Task PublishTaskUpdatedMessageAsync(Guid taskItemId, Guid updatedByUserId, string updateType);
}
