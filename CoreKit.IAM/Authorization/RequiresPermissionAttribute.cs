// CoreKit.IAM/Authorization/RequiresPermissionAttribute.cs
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.IAM.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequiresPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public RequiresPermissionAttribute(string permission)
        => _permission = permission;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Fast path: permission baked into JWT claims
        if (user.Claims.Any(c =>
            c.Type == ClaimConstants.Permission && c.Value == _permission))
            return;

        var services = context.HttpContext.RequestServices;
        var currentUserService = services.GetRequiredService<ICurrentUserService>();

        if (currentUserService.UserId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tenantContext = services.GetRequiredService<ITenantContext>();
        var permissionCache = services.GetRequiredService<IPermissionCacheService>();
        var permissionService = services.GetRequiredService<IPermissionService>();

        var userId = currentUserService.UserId.Value;
        var tenantId = tenantContext.TenantId;

        // FIX: repopulate cache on miss instead of only checking it
        var permissions = await permissionCache.GetAsync(userId, tenantId);

        if (permissions == null)
        {
            permissions = await permissionService.GetUserPermissionsAsync(userId, tenantId);
            await permissionCache.SetAsync(userId, tenantId, permissions);
        }

        if (!permissions.Contains(_permission))
            context.Result = new ForbidResult();
    }
}