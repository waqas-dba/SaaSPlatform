using System.Text.Json;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.Tenant.Interfaces;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value?.ToLower();

        if (path!.StartsWith("/swagger") ||
            path.StartsWith("/api/tenants/register"))
        {
            await _next(context);
            return;
        }

        if (!Guid.TryParse(
                context.Request.Headers["x-tenant-id"].FirstOrDefault(),
                out var tenantId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    ApiResponse<object>.FailResponse(
                        "Missing tenant",
                        "MISSING_TENANT")));
            return;
        }

        tenantContext.SetTenantId(tenantId);
        context.Items["TenantId"] = tenantId;

        await _next(context);
    }
}