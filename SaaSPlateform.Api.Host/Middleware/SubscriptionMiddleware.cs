using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.Billing.Interfaces;
using TenantKit.Abstractions;

namespace SaaSPlatform.Api.Host.Middleware;

public class SubscriptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionMiddleware> _logger;
    private static readonly string[] PublicPaths = { "/api/auth", "/api/tenants/register", "/swagger" };
    private static readonly Guid SystemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public SubscriptionMiddleware(RequestDelegate next, ILogger<SubscriptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISubscriptionAccessService subscriptionService, ITenantContext tenantContext)
    {
        if (PublicPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        var tenantId = tenantContext.TenantId;
        if (tenantId is null || tenantId == SystemTenantId || context.User.IsInRole("SuperAdmin"))
        {
            await _next(context);
            return;
        }

        try
        {
            if (!await subscriptionService.IsTenantAllowedAsync(tenantId.Value))
            {
                _logger.LogWarning("Subscription blocked for tenant: {TenantId}", tenantId);
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.FailResponse(
                    "Subscription expired, suspended, or payment pending",
                    "SUBSCRIPTION_BLOCKED",
                    new List<string> { "ACCESS_DENIED" },
                    context.TraceIdentifier);
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubscriptionMiddleware for tenant {TenantId}", tenantId);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var response = ApiResponse<object>.FailResponse(
                "Internal subscription validation error",
                "SUBSCRIPTION_ERROR",
                new List<string> { "INTERNAL_ERROR" },
                context.TraceIdentifier);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}