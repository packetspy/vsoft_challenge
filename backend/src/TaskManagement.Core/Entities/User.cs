namespace TaskManagement.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = [];
    public virtual ICollection<TaskItem> TaskItems { get; set; } = [];

    public bool HasPermission(string permission)
    {
        return Roles.Any(r => r.Permissions.Contains(permission));
    }
}
