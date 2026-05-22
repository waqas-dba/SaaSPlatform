using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;

namespace CoreKit.IAM.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequiresPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public RequiresPermissionAttribute(string permission)
        => _permission = permission;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.Result != null) return;

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check claim-based permissions (fast path from JWT)
        if (user.Claims.Any(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == _permission))
            return;

        // Full permission check via service
        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        var currentUser = context.HttpContext.RequestServices
            .GetRequiredService<ICurrentUserService>();

        if (currentUser.UserId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        Guid? tenantId;
        try
        {
            var scope = currentUser.GetTenantScope();
            tenantId = scope.IsGlobal ? null : scope.TenantId;
        }
        catch (UnauthorizedAccessException)
        {
            context.Result = new ForbidResult();
            return;
        }

        var hasPermission = await permissionService.HasPermissionAsync(
            currentUser.UserId.Value, tenantId, _permission);

        if (!hasPermission)
            context.Result = new ForbidResult();
    }
}