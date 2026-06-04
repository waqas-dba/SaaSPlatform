using Asp.Versioning;
using CoreKit.Catalog.Extensions;
using CoreKit.IAM.Extensions;
using CoreKit.Infrastructure.Extensions;
using CoreKit.Infrastructure.Middleware;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.Tenant.Extensions;
using CoreKit.Tenant.Middleware;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace SaaSPlatform.Tenant.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (ctx, token) =>
            {
                ctx.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                ctx.HttpContext.Response.ContentType =
                    "application/json";

                var retryAfter =
                    ctx.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out var retryDelay)
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
                cfg.PermitLimit =
                    builder.Environment.IsDevelopment() ? 50 : 10;

                cfg.Window = TimeSpan.FromMinutes(1);
            });

            options.AddFixedWindowLimiter("store-update", cfg =>
            {
                cfg.PermitLimit =
                    builder.Environment.IsDevelopment() ? 100 : 30;

                cfg.Window = TimeSpan.FromMinutes(1);
            });

            options.AddFixedWindowLimiter("product-create", cfg =>
            {
                cfg.PermitLimit =
                    builder.Environment.IsDevelopment() ? 100 : 30;

                cfg.Window = TimeSpan.FromMinutes(1);
            });

            options.AddFixedWindowLimiter("product-update", cfg =>
            {
                cfg.PermitLimit =
                    builder.Environment.IsDevelopment() ? 200 : 60;

                cfg.Window = TimeSpan.FromMinutes(1);
            });
        });

        builder.Services.AddHttpContextAccessor();

        var connStr =
            builder.Configuration.GetConnectionString("Postgres")
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

        builder.Services.AddCatalogModule(connStr, services =>
        {
            services.AddScoped<IStoreInfoProvider,
                TenantStoreInfoProvider>();
        });

        builder.Services.AddCoreKitControllers();

        builder.ValidateCoreKitConfiguration();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize =
                10 * 1024 * 1024;
        });
        // In the service configuration (before builder.Build()):
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
            options.InvalidModelStateResponseFactory = context =>
            {
                // This catches both body and route/query parameter validation errors
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
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"),
                new QueryStringApiVersionReader("api-version"));
        }).AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

        var app = builder.Build();

        // Reverse proxy support
        app.UseForwardedHeaders(
            new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto
            });

        // Production security
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Frame-Options"] =
                "DENY";

            context.Response.Headers["X-Content-Type-Options"] =
                "nosniff";

            context.Response.Headers["Referrer-Policy"] =
                "strict-origin-when-cross-origin";

            context.Response.Headers["X-Permitted-Cross-Domain-Policies"] =
                "none";

            context.Response.Headers["X-XSS-Protection"] =
                "0";

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

        app.UseAuthorization();

        app.MapGet("/ping", () => "pong");

        app.MapControllers();

        await app.RunAsync();
    }
}