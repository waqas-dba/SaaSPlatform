using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SaaSPlatform.Core.IAM.Interfaces;

namespace SaaSPlatform.Api.Host.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class TenantPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public TenantPermissionAttribute(string permission) => _permission = permission;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. SuperAdmin always passes
        if (context.HttpContext.User.IsInRole("SuperAdmin"))
            return;

        // 2. Tenant from middleware
        var tenantIdObj = context.HttpContext.Items["TenantId"];
        if (tenantIdObj is not Guid tenantId)
        {
            context.Result = new UnauthorizedObjectResult("Missing tenant context.");
            return;
        }

        // 3. Authenticated user
        var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult("User not authenticated.");
            return;
        }

        // 4. Permission check
        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var hasPerm = await permissionService.HasPermissionAsync(userId, tenantId, _permission);
        if (!hasPerm)
        {
            context.Result = new ForbidResult();
        }
    }
}