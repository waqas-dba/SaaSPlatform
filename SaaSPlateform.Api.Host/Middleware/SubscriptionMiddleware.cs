using Microsoft.AspNetCore.Http;
using SaaSPlatform.Core.Billing.Interfaces;

namespace SaaSPlatform.Api.Host.Middleware;

public class SubscriptionMiddleware
{
    private readonly RequestDelegate _next;

    public SubscriptionMiddleware(RequestDelegate next)
    {
        _next = next;
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

        var isAllowed = await subscriptionService.IsTenantAllowedAsync(tenantId);

        if (!isAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
            await context.Response.WriteAsync("Subscription Suspended");
            return;
        }

        await _next(context);
    }
}