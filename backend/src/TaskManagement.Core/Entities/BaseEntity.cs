namespace TaskManagement.Core.Entities;

public abstract class BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}