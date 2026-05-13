using AuthCoreKit.IAM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SaaSPlatform.Api.Host.Authorization;

public class TenantPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public TenantPermissionAttribute(string permission)
        => _permission = permission;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.IsInRole("SuperAdmin"))
            return;

        var tenantId = context.HttpContext.Items["TenantId"] as Guid?;
        if (tenantId is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = context.HttpContext.User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userId, out var uid))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var perm = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        if (!await perm.HasPermissionAsync(uid, tenantId.Value, _permission))
        {
            context.Result = new ForbidResult();
        }
    }
}