namespace TaskManagement.Application.Constants;

public static class Permissions
{
    public const string CanCreateTask = "CanCreateTask";
    public const string CanUpdateTask = "CanUpdateTask";
    public const string CanDeleteTask = "CanDeleteTask";
    public const string CanAssignTask = "CanAssignTask";

    public static readonly List<string> AllPermissions =
    [
        CanCreateTask, CanUpdateTask, CanDeleteTask, CanAssignTask
    ];
}