using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using CoreKit.Tenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.Tenant.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("x-tenant-id", out var tid) &&
            Guid.TryParse(tid, out var parsed))
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var currentUser = context.RequestServices.GetRequiredService<ICurrentUserService>();
                if (!currentUser.IsSuperAdmin && currentUser.UserId != null)
                {
                    var db = context.RequestServices.GetRequiredService<IamDbContext>();
                    var belongs = await db.UserRoles.IgnoreQueryFilters()
                        .AnyAsync(ur => ur.UserId == currentUser.UserId.Value
                                     && ur.TenantId == parsed);
                    if (!belongs)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Access denied to this tenant.");
                        return;
                    }
                }
            }

            var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();
            ((TenantContext)tenantContext).SetTenant(parsed);
        }

        await _next(context);
    }
}