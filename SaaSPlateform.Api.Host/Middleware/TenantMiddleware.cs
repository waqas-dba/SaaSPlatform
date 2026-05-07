using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Common.Responses;
using System.Text.Json;

namespace SaaSPlateform.Api.Host.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext context,
        ITenantAccessService tenantAccessService,
        ITenantContext tenantContext)
    {
        var tenantHeader = context.Request.Headers["X-Tenant-ID"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantHeader))
        {
            await WriteError(context,
                400,
                "Tenant header missing",
                "TENANT_HEADER_MISSING");

            return;
        }

        if (!Guid.TryParse(tenantHeader, out var tenantId))
        {
            await WriteError(context,
                400,
                "Invalid tenant id",
                "TENANT_INVALID_ID");

            return;
        }

        var exists = await tenantAccessService
            .TenantExistsAsync(tenantId);

        if (!exists)
        {
            await WriteError(context,
                404,
                "Tenant not found",
                "TENANT_NOT_FOUND");

            return;
        }

        tenantContext.TenantId = tenantId;

        await _next(context);
    }

    private static async Task WriteError(
        HttpContext context,
        int status,
        string message,
        string code)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.FailResponse(
            message,
            code);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}