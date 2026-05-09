using System.Text.Json;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.Billing.Interfaces;

namespace SaaSPlatform.Api.Host.Middleware;

public class SubscriptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionMiddleware> _logger;

    private static readonly string[] PublicPaths =
    {
        "/api/auth",
        "/api/tenants/register",
        "/swagger"
    };

    private static readonly Guid SystemTenantId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public SubscriptionMiddleware(RequestDelegate next, ILogger<SubscriptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISubscriptionAccessService subscriptionService)
    {
        // 1. Skip for public paths
        if (PublicPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        var tenantIdObj = context.Items["TenantId"];
        if (tenantIdObj is not Guid tenantId)
        {
            await _next(context);
            return;
        }

        // 2. Skip for the System Tenant (the tenant ID itself)
        if (tenantId == SystemTenantId)
        {
            await _next(context);
            return;
        }

        // 3. Skip if the authenticated user is a SuperAdmin
        if (context.User.IsInRole("SuperAdmin"))
        {
            await _next(context);
            return;
        }

        try
        {
            var isAllowed = await subscriptionService.IsTenantAllowedAsync(tenantId);
            if (!isAllowed)
            {
                _logger.LogWarning("Subscription blocked for tenant: {TenantId}", tenantId);
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.FailResponse(
                    message: "Subscription expired, suspended, or payment pending",
                    code: "SUBSCRIPTION_BLOCKED",
                    errors: new List<string> { "ACCESS_DENIED" },
                    traceId: context.TraceIdentifier
                );
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
                message: "Internal subscription validation error",
                code: "SUBSCRIPTION_ERROR",
                errors: new List<string> { "INTERNAL_ERROR" },
                traceId: context.TraceIdentifier
            );
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}