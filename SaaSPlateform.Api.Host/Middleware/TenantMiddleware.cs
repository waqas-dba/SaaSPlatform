using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.Tenant.Constants;
using SaaSPlatform.Core.Tenant.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace SaaSPlatform.Api.Host.Middleware;

/// <summary>
/// Resolves tenant from request header.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantAccessService tenantAccessService,
        ITenantContext tenantContext)
    {
        // =============================================
        // Skip swagger
        // =============================================

        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // =============================================
        // Read tenant header
        // =============================================

        var tenantHeader =
            context.Request.Headers[
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

        // =============================================
        // Validate Guid
        // =============================================

        if (!Guid.TryParse(tenantHeader, out var tenantId))
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Invalid tenant id");

            return;
        }

        // =============================================
        // Validate tenant exists
        // =============================================

        var exists =
            await tenantAccessService
                .TenantExistsAsync(tenantId);

        if (!exists)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status404NotFound,
                "Tenant not found");

            return;
        }

        // =============================================
        // SECURITY CHECK
        // Prevent cross tenant access
        // =============================================

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tokenTenant =
                context.User.FindFirstValue("tenantId");

            if (tokenTenant != tenantId.ToString())
            {
                await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Tenant mismatch");

                return;
            }
        }

        // =============================================
        // Store tenant context
        // =============================================

        tenantContext.TenantId = tenantId;

        context.Items[
            TenantConstants.TenantContextKey]
            = tenantId;

        await _next(context);
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;

        context.Response.ContentType = "application/json";

        var response =
            ApiResponse<object>.FailResponse(
                message,
                statusCode.ToString());

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}