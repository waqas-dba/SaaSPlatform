using System.Threading.RateLimiting;
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.ContentType = "application/json";

        var retryAfter = ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryDelay)
            ? (int)retryDelay.TotalSeconds
            : 60;

        ctx.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();

        await ctx.HttpContext.Response.WriteAsync(
            $"{{\"success\":false," +
            $"\"errorCode\":\"RATE_LIMITED\"," +
            $"\"message\":\"Too many requests. Please wait {retryAfter} seconds before trying again.\"}}",
            token);
    };

    options.AddFixedWindowLimiter("login", cfg =>
    {
        cfg.PermitLimit = 5;
        cfg.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("tenant_register", cfg =>
    {
        cfg.PermitLimit = 3;
        cfg.Window = TimeSpan.FromMinutes(10);
    });
});

builder.Services.AddHttpContextAccessor();

var connStr = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' is missing from configuration.");

builder.Services.AddCoreKitIAM(
    connStr,
    builder.Configuration.GetSection("Jwt"),
    options =>
    {
        options.AllowDefaultAdminSeed = builder.Environment.IsDevelopment();
        options.SuperAdminBypassPermissions = true;
        options.SuperAdminRoleName = "SuperAdmin";
    });

builder.Services.AddTenantKit(connStr, options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;
    options.EnableLegalInfo = true;
});

builder.Services.AddControllers();

var app = builder.Build();

// ── Startup validation ───────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("\n[BOOT] ===== SERVICE RESOLUTION CHECK =====");

try
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;

    // 1. No DB needed
    _ = sp.GetRequiredService<CoreKit.IAM.Interfaces.IEncryptionService>();
    Console.WriteLine("[BOOT] IEncryptionService         OK");

    // 2. Check DB connection explicitly with a 5-second timeout
    Console.WriteLine("[BOOT] Testing database connection ...");
    var iamDb = sp.GetRequiredService<CoreKit.IAM.Persistence.IamDbContext>();

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var canConnect = await iamDb.Database.CanConnectAsync(cts.Token);

    if (!canConnect)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[BOOT] DATABASE: CanConnectAsync returned FALSE");
        Console.WriteLine("[BOOT] Check your PostgreSQL connection string in appsettings.json");
        Console.WriteLine($"[BOOT] Connection string used: {app.Configuration.GetConnectionString("Postgres")}");
        Console.ResetColor();
        // Do NOT throw — let app start so you can still hit health endpoints
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[BOOT] DATABASE: connection OK");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Magenta;

        // 3. Only resolve DB-dependent services if DB is reachable
        _ = sp.GetRequiredService<CoreKit.IAM.Interfaces.IAuthService>();
        Console.WriteLine("[BOOT] IAuthService               OK");

        _ = sp.GetRequiredService<CoreKit.IAM.Interfaces.ICurrentUserService>();
        Console.WriteLine("[BOOT] ICurrentUserService        OK");

        _ = sp.GetRequiredService<CoreKit.Tenant.Services.IMutableTenantContext>();
        Console.WriteLine("[BOOT] IMutableTenantContext      OK");

        _ = sp.GetRequiredService<CoreKit.Tenant.Middleware.TenantResolutionMiddleware>();
        Console.WriteLine("[BOOT] TenantResolutionMiddleware OK");
    }

    Console.WriteLine("[BOOT] ===== BOOT CHECK COMPLETE =====\n");
}
catch (OperationCanceledException)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("[BOOT] DATABASE: connection TIMED OUT after 5 seconds");
    Console.WriteLine("[BOOT] PostgreSQL is not reachable — check:");
    Console.WriteLine("[BOOT]   1. Is PostgreSQL running?");
    Console.WriteLine("[BOOT]   2. Is the Host/Port correct in appsettings.json?");
    Console.WriteLine("[BOOT]   3. Is the firewall/docker port open?");
    Console.WriteLine($"[BOOT]   Connection string: {app.Configuration.GetConnectionString("Postgres")}");
    Console.ResetColor();
    // Do NOT throw — print and continue so you see the full picture
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[BOOT] FAILED: {ex.GetType().Name}");
    Console.WriteLine($"[BOOT] {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"[BOOT] Inner: {ex.InnerException.Message}");
    Console.WriteLine($"[BOOT] Connection string: {app.Configuration.GetConnectionString("Postgres")}");
    Console.ResetColor();
}
Console.ResetColor();

app.UseMiddleware<RequestTracingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();