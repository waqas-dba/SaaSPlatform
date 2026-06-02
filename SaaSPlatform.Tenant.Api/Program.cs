using CoreKit.Catalog.Abstractions;
using CoreKit.Catalog.Extensions;
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using SaaSPlatform.Admin.Api.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// RATE LIMITING
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.ContentType = "application/json";

        var retryAfter = ctx.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryDelay)
            ? (int)retryDelay.TotalSeconds
            : 60;

        ctx.HttpContext.Response.Headers["Retry-After"] =
            retryAfter.ToString();

        await ctx.HttpContext.Response.WriteAsync(
            $"{{\"success\":false," +
            $"\"errorCode\":\"RATE_LIMITED\"," +
            $"\"message\":\"Too many requests. " +
            $"Please wait {retryAfter} seconds.\"}}",
            token);
    };

    options.AddFixedWindowLimiter("store-create", cfg =>
    {
        cfg.PermitLimit = builder.Environment.IsDevelopment() ? 50 : 10;
        cfg.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("store-update", cfg =>
    {
        cfg.PermitLimit = builder.Environment.IsDevelopment() ? 100 : 30;
        cfg.Window = TimeSpan.FromMinutes(1);
    });

    // NEW: protect product creation
    options.AddFixedWindowLimiter("product-create", cfg =>
    {
        cfg.PermitLimit = builder.Environment.IsDevelopment() ? 100 : 30;
        cfg.Window = TimeSpan.FromMinutes(1);
    });
});

// ==========================================
// HTTP CONTEXT
// ==========================================
builder.Services.AddHttpContextAccessor();

// ==========================================
// DATABASE CONNECTION
// ==========================================
var connStr = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' is missing from configuration.");

// ==========================================
// IAM — NO BYPASS, PURE PERMISSION-BASED
// ==========================================
builder.Services.AddCoreKitIAM(
    connStr,
    builder.Configuration.GetSection("Jwt"));

// ==========================================
// TENANT KIT
// ==========================================
builder.Services.AddTenantKit(connStr, options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;
    options.EnableLegalInfo = true;
});

// ==========================================
// CATALOG MODULE — MANDATORY IStoreInfoProvider
// ==========================================
builder.Services.AddCatalogModule(connStr, services =>
{
    // Use the Tenant DB to resolve store info safely
    services.AddScoped<IStoreInfoProvider, TenantStoreInfoProvider>();
});

// ==========================================
// CONTROLLERS
// ==========================================
builder.Services.AddCoreKitControllers();

// ==========================================
// STARTUP CONFIGURATION VALIDATION
// ==========================================
builder.ValidateCoreKitConfiguration();

// ==========================================
// KESTREL
// ==========================================
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

// ==========================================
// BUILD APP
// ==========================================
var app = builder.Build();

// ==========================================
// SECURITY HEADERS
// ==========================================
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["X-XSS-Protection"] = "0";
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=(), interest-cohort=()";

    await next();
});

// ==========================================
// BOOT CHECK — STILL NEEDS REPLACEMENT WITH HEALTH CHECKS
// ==========================================
await app.PerformBootCheckAsync();

// ==========================================
// MIDDLEWARE PIPELINE
// ==========================================
app.UseMiddleware<ExceptionMiddleware>();        // outermost error handler
app.UseMiddleware<RequestTracingMiddleware>();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

// ==========================================
// ENDPOINTS
// ==========================================
app.MapGet("/ping", () => "pong");
app.MapControllers();

// ==========================================
// RUN
// ==========================================
app.Run();