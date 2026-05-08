// SaaSPlatform.Api.Host/Middleware/TenantMiddleware.cs
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Common.Responses;
using System.Text.Json;

namespace SaaSPlateform.Api.Host.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    // Paths that don’t require tenant header
    private static readonly string[] PublicPaths =
    {
        "/api/tenants/register",
        "/api/auth/login",          // adjust if your auth endpoint differs
        "/api/auth/refresh",
        "/api/auth/admin-login",    // if you create admin login later
        "/api/marketplace"          // future marketplace (starts with)
    };

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext context,
        ITenantAccessService tenantAccessService,
        ITenantContext tenantContext)
    {
        // ---- Skip tenant validation for public endpoints ----
        if (IsPublicPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // ---- Normal tenant resolution ----
        var tenantHeader = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tenantHeader))
        {
            await WriteError(context, 400, "Tenant header missing", "TENANT_HEADER_MISSING");
            return;
        }

        if (!Guid.TryParse(tenantHeader, out var tenantId))
        {
            await WriteError(context, 400, "Invalid tenant id", "TENANT_INVALID_ID");
            return;
        }

        var exists = await tenantAccessService.TenantExistsAsync(tenantId);
        if (!exists)
        {
            await WriteError(context, 404, "Tenant not found", "TENANT_NOT_FOUND");
            return;
        }

        tenantContext.TenantId = tenantId;
        await _next(context);
    }

    private static bool IsPublicPath(string path)
    {
        foreach (var publicPath in PublicPaths)
        {
            if (path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static async Task WriteError(HttpContext context, int status, string message, string code)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.FailResponse(message, code);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}