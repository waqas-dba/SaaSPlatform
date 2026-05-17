using System.Text.Json;
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

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[BOOT] TenantResolutionMiddleware instantiated OK");
        Console.ResetColor();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var traceId = context.Items.TryGetValue("TraceId", out var t)
            ? t?.ToString() : "??";
        var path = context.Request.Path.Value?.ToLowerInvariant();

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{traceId}] >>> ENTERING TenantResolutionMiddleware — path={path}");
        Console.ResetColor();

        // ── Auth bypass ───────────────────────────────────────────────────────
        if (path != null &&
            (path.StartsWith("/api/auth/login") ||
             path.StartsWith("/api/auth/refresh") ||
             path.StartsWith("/api/auth/logout")))
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[{traceId}] TenantMW: auth path — bypassing");
            Console.ResetColor();
            await next(context);
            return;
        }

        // ── No header ─────────────────────────────────────────────────────────
        if (!context.Request.Headers.TryGetValue("x-tenant-id", out var tid))
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[{traceId}] TenantMW: no x-tenant-id — passing through");
            Console.ResetColor();
            await next(context);
            return;
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{traceId}] TenantMW: x-tenant-id={tid}");
        Console.ResetColor();

        // ── Parse ─────────────────────────────────────────────────────────────
        if (!Guid.TryParse(tid, out var tenantId))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{traceId}] TenantMW: invalid GUID — 400");
            Console.ResetColor();
            await WriteAsync(context, 400, "BAD_REQUEST",
                "The x-tenant-id header value is not a valid identifier. " +
                "Please provide a valid tenant ID.");
            return;
        }

        // ── DB lookup ─────────────────────────────────────────────────────────
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{traceId}] TenantMW: querying DB for tenant {tenantId} ...");
        Console.ResetColor();

        TenantEntity? tenant;
        try
        {
            tenant = await _db.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == tenantId,
                    context.RequestAborted);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(tenant == null
                ? $"[{traceId}] TenantMW: tenant NOT found in DB"
                : $"[{traceId}] TenantMW: tenant found — Name={tenant.Name} Status={tenant.Status} IsDeleted={tenant.IsDeleted}");
            Console.ResetColor();
        }
        catch (OperationCanceledException)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{traceId}] TenantMW: cancelled during DB lookup");
            Console.ResetColor();
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
            return;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{traceId}] TenantMW: DB EXCEPTION — {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
            _logger.LogError(ex,
                "[{TraceId}] DB error resolving tenant {TenantId}", traceId, tenantId);
            await WriteAsync(context, 500, "SERVER_ERROR",
                "Something went wrong on our end. Please try again in a moment.");
            return;
        }

        // ── Tenant validation ─────────────────────────────────────────────────
        if (tenant == null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{traceId}] TenantMW: tenant null — 404");
            Console.ResetColor();
            await WriteAsync(context, 404, "TENANT_NOT_FOUND",
                "The tenant specified in x-tenant-id was not found. " +
                "Please check the tenant ID and try again.");
            return;
        }

        if (tenant.IsDeleted)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{traceId}] TenantMW: tenant deleted — 410");
            Console.ResetColor();
            await WriteAsync(context, 410, "TENANT_DELETED",
                "This tenant account has been permanently removed and is no longer accessible.");
            return;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{traceId}] TenantMW: tenant inactive ({tenant.Status}) — 403");
            Console.ResetColor();

            var msg = tenant.Status switch
            {
                TenantStatus.Pending =>
                    "This tenant account is pending approval and cannot be accessed yet. " +
                    "Please contact support if you believe this is a mistake.",
                TenantStatus.Suspended =>
                    "This tenant account has been suspended. " +
                    "Please contact support to resolve this.",
                TenantStatus.Rejected =>
                    "This tenant account registration was not approved.",
                TenantStatus.Archived =>
                    "This tenant account has been archived and is no longer active.",
                _ =>
                    $"This tenant account is currently {tenant.Status} and cannot be accessed."
            };

            await WriteAsync(context, 403, "TENANT_INACTIVE", msg);
            return;
        }

        // ── Set context ───────────────────────────────────────────────────────
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{traceId}] TenantMW: tenant active — setting context");
        Console.ResetColor();

        _tenantContext.SetTenant(tenantId);

        // ── Cross-tenant check ────────────────────────────────────────────────
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{traceId}] TenantMW: IsAuthenticated={context.User.Identity?.IsAuthenticated}");
        Console.ResetColor();

        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[{traceId}] TenantMW: IsSuperAdmin={_currentUser.IsSuperAdmin} UserId={_currentUser.UserId}");
                Console.ResetColor();

                if (!_currentUser.IsSuperAdmin && _currentUser.UserId != null)
                {
                    var scope = _currentUser.GetTenantScope();

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"[{traceId}] TenantMW: scope IsGlobal={scope.IsGlobal} ScopeTenantId={scope.TenantId}");
                    Console.ResetColor();

                    if (!scope.IsGlobal && scope.TenantId != tenantId)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[{traceId}] TenantMW: cross-tenant blocked — 403");
                        Console.ResetColor();
                        _logger.LogWarning(
                            "[{TraceId}] User {UserId} cross-tenant attempt to {TenantId}",
                            traceId, _currentUser.UserId, tenantId);
                        await WriteAsync(context, 403, "TENANT_ACCESS_DENIED",
                            "You do not have access to this tenant. " +
                            "Please use the credentials associated with this account.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{traceId}] TenantMW: EXCEPTION in scope check — {ex.GetType().Name}: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex,
                    "[{TraceId}] Scope check failed for user {UserId}",
                    traceId, _currentUser.UserId);
                await WriteAsync(context, 401, "AUTH_VALIDATION_FAILED",
                    "Your session could not be validated. Please log in again.");
                return;
            }
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{traceId}] TenantMW: all checks passed — calling next");
        Console.ResetColor();

        await next(context);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{traceId}] <<< LEAVING TenantResolutionMiddleware — status={context.Response.StatusCode}");
        Console.ResetColor();
    }

    private static Task WriteAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message)
    {
        if (context.Response.HasStarted)
            return Task.CompletedTask;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(
            new { success = false, errorCode, message },
            JsonOptions);

        return context.Response.WriteAsync(body);
    }
}