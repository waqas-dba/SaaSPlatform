using System.Text.Json;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using CoreKit.Tenant.Persistence;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreKit.Tenant.Middleware;

public class TenantResolutionMiddleware : IMiddleware
{
    private readonly TenantDbContext _db;
    private readonly IMutableTenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly HashSet<string> BypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/refresh",
        "/api/auth/logout"
    };

    public TenantResolutionMiddleware(
        TenantDbContext db,
        IMutableTenantContext tenantContext,
        ICurrentUserService currentUser,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var traceId = context.Items.TryGetValue("TraceId", out var t)
            ? t?.ToString() : "??";
        var path = context.Request.Path.Value ?? string.Empty;

        if (BypassPaths.Contains(path))
        {
            _logger.LogDebug("[{TraceId}] Tenant middleware bypassed for auth path {Path}", traceId, path);
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("x-tenant-id", out var tid))
        {
            _logger.LogDebug("[{TraceId}] No x-tenant-id header — passing through", traceId);
            await next(context);
            return;
        }

        if (!Guid.TryParse(tid, out var tenantId))
        {
            _logger.LogWarning("[{TraceId}] Invalid x-tenant-id value: {Value}", traceId, tid.ToString());
            await WriteAsync(context, 400, "BAD_REQUEST",
                "The x-tenant-id header value is not a valid identifier.");
            return;
        }

        TenantEntity? tenant;
        try
        {
            tenant = await _db.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == tenantId, context.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[{TraceId}] Request cancelled during tenant lookup", traceId);
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Database error resolving tenant {TenantId}", traceId, tenantId);
            await WriteAsync(context, 500, "SERVER_ERROR",
                "Something went wrong on our end. Please try again in a moment.");
            return;
        }

        if (tenant == null)
        {
            _logger.LogWarning("[{TraceId}] Tenant {TenantId} not found", traceId, tenantId);
            await WriteAsync(context, 404, "TENANT_NOT_FOUND",
                "The tenant specified in x-tenant-id was not found.");
            return;
        }

        if (tenant.IsDeleted)
        {
            _logger.LogWarning("[{TraceId}] Tenant {TenantId} is deleted", traceId, tenantId);
            await WriteAsync(context, 410, "TENANT_DELETED",
                "This tenant account has been permanently removed and is no longer accessible.");
            return;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            _logger.LogWarning("[{TraceId}] Tenant {TenantId} is not active — status={Status}",
                traceId, tenantId, tenant.Status);

            var msg = tenant.Status switch
            {
                TenantStatus.Pending => "This tenant account is pending approval and cannot be accessed yet.",
                TenantStatus.Suspended => "This tenant account has been suspended. Please contact support.",
                TenantStatus.Rejected => "This tenant account registration was not approved.",
                TenantStatus.Archived => "This tenant account has been archived and is no longer active.",
                _ => $"This tenant account is currently {tenant.Status} and cannot be accessed."
            };

            await WriteAsync(context, 403, "TENANT_INACTIVE", msg);
            return;
        }

        _tenantContext.SetTenant(tenantId);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                // Platform users with cross-tenant permission bypass tenant membership check
                var hasCrossTenantAccess = _currentUser.HasPermission(Permissions.Platform.ViewAllTenants);

                if (!hasCrossTenantAccess && _currentUser.UserId != null)
                {
                    var scope = _currentUser.GetTenantScope();
                    if (!scope.IsGlobal && scope.TenantId != tenantId)
                    {
                        _logger.LogWarning(
                            "[{TraceId}] Cross-tenant attempt by user {UserId} to tenant {TenantId}",
                            traceId, _currentUser.UserId, tenantId);

                        await WriteAsync(context, 403, "TENANT_ACCESS_DENIED",
                            "You do not have access to this tenant.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Scope check failed for user {UserId}",
                    traceId, _currentUser.UserId);

                await WriteAsync(context, 401, "AUTH_VALIDATION_FAILED",
                    "Your session could not be validated. Please log in again.");
                return;
            }
        }

        await next(context);
    }

    private static Task WriteAsync(
        HttpContext context, int statusCode, string errorCode, string message)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(
                new { success = false, errorCode, message },
                JsonOptions));
    }
}