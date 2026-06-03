using Microsoft.AspNetCore.Authorization;
using CoreKit.IAM.Interfaces;

namespace CoreKit.IAM.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserPermissions _permissions;

    public PermissionHandler(ICurrentUserPermissions permissions)
    {
        _permissions = permissions;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (_permissions.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}