using CoreKit.IAM.Constants;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreKit.Tenant.Middleware;

public class TenantAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantAuthorizationMiddleware> _logger;

    public TenantAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<TenantAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
            && tenantContext.TenantId.HasValue)
        {
            var user = context.User;

            bool isPlatformAdmin = user.IsInRole("PlatformAdmin") ||
                                   user.HasClaim(c => c.Type == ClaimConstants.Permission &&
                                                       (c.Value == "platform.tenants.view" ||
                                                        c.Value == "platform.tenants.manage"));

            if (!isPlatformAdmin)
            {
                var userTenantClaim = user.FindFirst(ClaimConstants.TenantId)?.Value;
                if (userTenantClaim == null ||
                    !Guid.TryParse(userTenantClaim, out var userTenantId) ||
                    userTenantId != tenantContext.TenantId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to access tenant {RequestedTenant} but belongs to tenant {UserTenant}",
                        user.FindFirst(ClaimConstants.UserId)?.Value,
                        tenantContext.TenantId,
                        userTenantClaim);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"success\":false,\"errorCode\":\"FORBIDDEN\",\"message\":\"You do not have access to this tenant.\"}");
                    return;
                }
            }
        }

        await _next(context);
    }
}