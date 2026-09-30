using Asp.Versioning;
using CoreKit.Catalog.Extensions;
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.Subscription.Extensions;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using CoreKit.Tenant.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaaSPlatform.Admin.Api.Validators;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Rate Limiting -------------------
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

    options.AddFixedWindowLimiter("admin-operations", cfg =>
    {
        cfg.PermitLimit = 30;
        cfg.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("product-create", cfg =>
    {
        cfg.PermitLimit = builder.Environment.IsDevelopment() ? 100 : 20;
        cfg.Window = TimeSpan.FromMinutes(1);
    });
});

// ------------------- FluentValidation -------------------
builder.Services.AddValidatorsFromAssemblyContaining<AdminCreateUserRequestValidator>();

// ------------------- Core Services -------------------
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

builder.Services.AddSubscriptionModule(connStr);
builder.Services.AddCoreKitControllers();
builder.ValidateCoreKitConfiguration();

builder.Services.AddCatalogModule(connStr, services =>
{
    services.AddScoped<IStoreInfoProvider, TenantStoreInfoProvider>();
});

// ------------------- Kestrel -------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
});

// ------------------- JSON Options -------------------
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// ------------------- Model Validation Errors -------------------
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(x => new
            {
                field = e.Key,
                message = x.ErrorMessage
            }))
            .ToList();

        return new ObjectResult(new
        {
            success = false,
            errorCode = "VALIDATION_ERROR",
            message = "One or more validation errors occurred.",
            errors
        })
        {
            StatusCode = 422
        };
    };
});


// Configure logging
builder.Services.AddLogging();

// ------------------- API Versioning -------------------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ------------------- App Pipeline -------------------
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

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

app.UseMiddleware<RequestTracingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<TenantAuthorizationMiddleware>();

app.UseAuthorization();

app.MapGet("/ping", () => "pong");
app.MapControllers();

// Inject logger
app.MapGet("/status", (ILogger<Program> logger) =>
{
    logger.LogInformation("Application running");
    return Results.Ok(new { Status = "OK" });
});

app.Run();