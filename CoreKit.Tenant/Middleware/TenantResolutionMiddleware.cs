using CoreKit.IAM.Constants;
using CoreKit.SharedKernel.Tenancy;   // IMutableTenantContext lives here after deletion of duplicates
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreKit.Tenant.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IMutableTenantContext tenantContext)
    {
        Guid? tenantId = ResolveTenant(context);

        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            tenantContext.SetTenant(tenantId.Value);
            _logger.LogDebug("Tenant resolved: {TenantId}", tenantId.Value);
        }
        else
        {
            tenantContext.SetTenant(null);
            _logger.LogWarning(
                "Tenant resolution failed for path {Path} | User: {User}",
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anonymous");
        }

        await _next(context);
    }

    private static Guid? ResolveTenant(HttpContext context)
    {
        // 1. Header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenant)
            && Guid.TryParse(headerTenant, out var id))
            return id;

        // 2. JWT claim
        var claimTenant = context.User?.FindFirst(ClaimConstants.TenantId)?.Value;
        if (Guid.TryParse(claimTenant, out var claimId))
            return claimId;

        // 3. Subdomain (future implementation)
        // var host = context.Request.Host.Host;
        // var parts = host.Split('.');
        // if (parts.Length >= 3) { … }

        return null;
    }
}