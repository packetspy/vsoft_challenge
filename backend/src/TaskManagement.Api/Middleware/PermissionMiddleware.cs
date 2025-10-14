using System.Security.Claims;
using TaskManagement.Application.Constants;

namespace TaskManagement.API.Middleware;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionMiddleware> _logger;

    public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var permissionAttribute = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (permissionAttribute != null)
        {
            var user = context.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var userPermissions = user.FindAll("permission").Select(c => c.Value);
            var requiredPermission = permissionAttribute.Permission;

            if (!userPermissions.Contains(requiredPermission))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync($"Forbidden: Requires {requiredPermission} permission");
                return;
            }
        }

        await _next(context);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }
}