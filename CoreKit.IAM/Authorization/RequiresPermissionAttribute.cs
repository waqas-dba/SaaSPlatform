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

        if (user.Claims.Any(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == _permission))
            return;

        var permissionService =
            context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

        var currentUser =
            context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        var tenantContext =
            context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();

        if (currentUser.UserId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = await permissionService.HasPermissionAsync(
            currentUser.UserId.Value,
            tenantContext.TenantId,
            _permission);

        if (!hasPermission)
            context.Result = new ForbidResult();
    }
}