// CoreKit.Tenant/Middleware/TenantResolutionMiddleware.cs
using CoreKit.IAM.Interfaces;
using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.Tenant.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("x-tenant-id", out var tid) &&
            Guid.TryParse(tid, out var parsed))
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var currentUser = context.RequestServices
                    .GetRequiredService<ICurrentUserService>();

                // FIX: Use ICurrentUserService.GetTenantScope() rather than
                // querying IamDbContext directly. This removes the hard dependency
                // of CoreKit.Tenant middleware on CoreKit.IAM's DbContext.
                // Non-SuperAdmin users must belong to the requested tenant.
                if (!currentUser.IsSuperAdmin && currentUser.UserId != null)
                {
                    var userBelongsToTenant = UserBelongsToTenant(currentUser, parsed);
                    if (!userBelongsToTenant)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Access denied to this tenant.");
                        return;
                    }
                }
            }

            // FIX: Resolve IMutableTenantContext so no unsafe cast is needed.
            var tenantContext = context.RequestServices
                .GetRequiredService<IMutableTenantContext>();
            tenantContext.SetTenant(parsed);
        }

        await _next(context);
    }

    // Checks membership using the claims already present in the token,
    // avoiding an extra DB round-trip on every request.
    private static bool UserBelongsToTenant(ICurrentUserService currentUser, Guid tenantId)
    {
        try
        {
            var scope = currentUser.GetTenantScope();
            // Global scope = SuperAdmin (already excluded above, but guard anyway).
            if (scope.IsGlobal) return true;
            return scope.TenantId == tenantId;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}