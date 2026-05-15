using CoreKit.IAM.Interfaces;   // ✅ added for tenant validation
using CoreKit.Tenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CoreKit.Tenant.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        IUserManagementService? userManagementService,  // ✅ optional, may be null if IAM not registered
        ICurrentUserService? currentUserService)
    {
        if (context.Request.Headers.TryGetValue("x-tenant-id", out var tid) &&
            Guid.TryParse(tid, out var parsed))
        {
            // ✅ If the user is authenticated, verify they actually belong to the requested tenant
            if (context.User.Identity?.IsAuthenticated == true &&
                userManagementService != null &&
                currentUserService?.UserId != null)
            {
                var user = await userManagementService.GetUserByIdAsync(
                    currentUserService.UserId.Value, parsed);

                if (user == null)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access denied to the requested tenant.");
                    return;
                }
            }

            ((TenantContext)tenantContext).SetTenant(parsed);
        }

        await _next(context);
    }
}