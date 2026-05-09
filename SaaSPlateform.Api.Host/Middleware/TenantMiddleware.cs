using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.IAM.Constants;
using SaaSPlatform.Core.Tenant.Constants;
using SaaSPlatform.Core.Tenant.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace SaaSPlatform.Api.Host.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] PublicPaths =
    {
        "/swagger",
        "/api/auth/login",
        "/api/auth/refresh",
        "/api/tenants/register"
    };

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantAccessService tenantAccessService,
        ITenantContext tenantContext)
    {
        // Skip public paths
        if (PublicPaths.Any(p =>
            context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        // Read tenant header
        var tenantHeader = context.Request.Headers[
            TenantConstants.TenantHeader]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantHeader))
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Tenant header missing");

            return;
        }

        if (!Guid.TryParse(tenantHeader, out var tenantId))
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Invalid tenant id");

            return;
        }

        // IMPORTANT:
        // STORE TENANT EARLY
        tenantContext.TenantId = tenantId;

        context.Items[TenantConstants.TenantContextKey] = tenantId;

        // SuperAdmin bypass
        var isSuperAdmin =
            context.User.IsInRole("SuperAdmin");

        // Tenant existence check
        if (!isSuperAdmin &&
            !context.Request.Path.StartsWithSegments("/api/admin"))
        {
            var exists = await tenantAccessService
                .TenantExistsAsync(tenantId);

            if (!exists)
            {
                await WriteErrorAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    "Tenant not found");

                return;
            }
        }

        // Cross tenant validation
        if (!isSuperAdmin &&
            context.User.Identity?.IsAuthenticated == true)
        {
            var tokenTenant = context.User
                .FindFirstValue(ClaimConstants.TenantId);

            if (!string.IsNullOrWhiteSpace(tokenTenant) &&
                tokenTenant != tenantId.ToString())
            {
                await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Tenant mismatch");

                return;
            }
        }

        await _next(context);
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;

        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>
            .FailResponse(
                message,
                statusCode.ToString());

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}