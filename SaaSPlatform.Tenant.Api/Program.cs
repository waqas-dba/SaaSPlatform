using CoreKit.Catalog.Abstractions;
using CoreKit.Catalog.Extensions;
using CoreKit.Catalog.Services;
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.ContentType = "application/json";

        var retryAfter = ctx.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryDelay)
            ? (int)retryDelay.TotalSeconds
            : 60;

        ctx.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();

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

    options.AddFixedWindowLimiter("product-create", cfg =>
    {
        cfg.PermitLimit = builder.Environment.IsDevelopment() ? 100 : 30;
        cfg.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("product-update", cfg =>
    {
        cfg.PermitLimit = builder.Environment.IsDevelopment() ? 200 : 60;
        cfg.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddHttpContextAccessor();

var connStr = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' is missing from configuration.");

builder.Services.AddCoreKitIAM(
    connStr,
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddTenantKit(connStr, options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;
    options.EnableLegalInfo = true;
});

// Use the shared canonical TenantStoreInfoProvider from CoreKit.Catalog.Services.
// No per-project copy needed — fixes the duplication across all three API projects.
builder.Services.AddCatalogModule(connStr, services =>
{
    services.AddScoped<IStoreInfoProvider, TenantStoreInfoProvider>();
});

builder.Services.AddCoreKitControllers();
builder.ValidateCoreKitConfiguration();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
});

var app = builder.Build();

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

await app.PerformBootCheckAsync();

// Correct order: RequestTracing is outermost so it captures the full
// request lifecycle including exceptions. ExceptionMiddleware sits inside
// it so unhandled exceptions are both logged by tracing and formatted by
// the exception handler.
app.UseMiddleware<RequestTracingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapGet("/ping", () => "pong");
app.MapControllers();

app.Run();