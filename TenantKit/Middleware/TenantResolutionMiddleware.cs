using TenantKit.Infrastructure;

namespace TenantKit.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        TenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue("x-tenant-id", out var tenantId))
        {
            if (Guid.TryParse(tenantId, out var parsed))
            {
                tenantContext.TenantId = parsed;
            }
        }

        await _next(context);
    }
}