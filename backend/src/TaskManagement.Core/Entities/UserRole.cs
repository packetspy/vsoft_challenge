namespace TaskManagement.Core.Entities;

public class UserRole
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}
