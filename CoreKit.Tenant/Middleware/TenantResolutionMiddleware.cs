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

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue("x-tenant-id", out var tid))
        {
            if (Guid.TryParse(tid, out var parsed))
            {
                ((TenantContext)tenantContext).SetTenant(parsed);
            }
        }

        await _next(context);
    }
}