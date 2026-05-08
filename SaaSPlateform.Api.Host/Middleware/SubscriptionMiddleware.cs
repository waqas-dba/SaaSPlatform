using System.Text.Json;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.Billing.Interfaces;

namespace SaaSPlatform.Api.Host.Middleware;

public class SubscriptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionMiddleware> _logger;

    public SubscriptionMiddleware(
        RequestDelegate next,
        ILogger<SubscriptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISubscriptionAccessService subscriptionService)
    {
        var tenantIdObj = context.Items["TenantId"];

        if (tenantIdObj is not Guid tenantId)
        {
            await _next(context);
            return;
        }

        try
        {
            var isAllowed = await subscriptionService
                .IsTenantAllowedAsync(tenantId);

            if (!isAllowed)
            {
                _logger.LogWarning(
                    "Subscription blocked for tenant: {TenantId}",
                    tenantId);

                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.FailResponse(
                    message: "Subscription expired, suspended, or payment pending",
                    code: "SUBSCRIPTION_BLOCKED",
                    errors: new List<string> { "ACCESS_DENIED" },
                    traceId: context.TraceIdentifier
                );

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response));

                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in SubscriptionMiddleware for tenant {TenantId}",
                tenantId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.FailResponse(
                message: "Internal subscription validation error",
                code: "SUBSCRIPTION_ERROR",
                errors: new List<string> { "INTERNAL_ERROR" },
                traceId: context.TraceIdentifier
            );

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));

            return;
        }
    }
}