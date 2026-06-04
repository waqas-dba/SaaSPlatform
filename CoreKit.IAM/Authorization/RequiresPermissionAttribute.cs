// FILE: CoreKit.IAM/Authorization/RequiresPermissionAttribute.cs
// FIX: Check permission cache before hitting the database.
//      Previously the attribute only checked JWT claims, then went straight
//      to the DB. Now it checks the in-memory cache layer first.

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

        // Fast path: permission is embedded in the JWT claim.
        if (user.Claims.Any(c =>
                c.Type == ClaimConstants.Permission &&
                c.Value == _permission))
            return;

        var currentUserService =
            context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        if (currentUserService.UserId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tenantContext =
            context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();

        // Second path: check the in-memory permission cache before touching the DB.
        // This avoids a DB round-trip on every request when the permission was not
        // embedded in the token (e.g. after a role change without re-login).
        var permissionCache =
            context.HttpContext.RequestServices.GetRequiredService<IPermissionCacheService>();

        var cached = await permissionCache.GetAsync(
            currentUserService.UserId.Value,
            tenantContext.TenantId);

        if (cached != null)
        {
            if (!cached.Contains(_permission))
                context.Result = new ForbidResult();

            // Permission found (or not) in cache — no DB needed.
            return;
        }

        // Final path: cache miss — delegate to PermissionService which will
        // query the DB and populate the cache for subsequent requests.
        var permissionService =
            context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

        var hasPermission = await permissionService.HasPermissionAsync(
            currentUserService.UserId.Value,
            tenantContext.TenantId,
            _permission);

        if (!hasPermission)
            context.Result = new ForbidResult();
    }
}