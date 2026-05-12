using AuthCoreKit.IAM.Extensions;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Api.Host.Middleware;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Application.Services;
using SaaSPlatform.Application.Validators.Auth;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Core.Billing.Services;
using SaaSPlatform.Core.Catalog.Interfaces;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Persistence.Repositories;
using SaaSPlatform.Infrastructure.Persistence.Seed;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.Billing;
using SaaSPlatform.Infrastructure.Services.Common;
using SaaSPlatform.Infrastructure.Services.Subscriptions;
using SaaSPlatform.Infrastructure.Services.TenantServices;
using SaaSPlatform.SharedKernel.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONTROLLERS + MODEL VALIDATION
// ======================================================
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
                .ToList();

            var response = ApiResponse<object>.FailResponse(
                message: "Validation failed",
                code: "VALIDATION_ERROR",
                errors: errors,
                traceId: context.HttpContext.TraceIdentifier);

            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// ======================================================
// FLUENT VALIDATION
// ======================================================
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// ======================================================
// JWT & IAM – GENERIC EXTENSION (auto‑registers IIamDbContext)
// ======================================================
builder.Services.AddAuthCoreKit<SaaSPlatformDbContext>(
    builder.Configuration.GetSection("Jwt"),
    options =>
    {
        options.LoginIdentifier = "phone";

        options.TenantResolver = ctx =>
        {
            var header = ctx.Request.Headers["x-tenant-id"].FirstOrDefault();
            return Guid.TryParse(header, out var tid) ? tid : null;
        };

        options.SuperAdminRoleName = "SuperAdmin";
        options.SuperAdminBypassPermissions = true;
    });

// ======================================================
// RATE LIMITING
// ======================================================
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("default", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 100;
        config.QueueLimit = 0;
    });
});

// ======================================================
// TENANT SERVICES (non‑IAM)
// ======================================================
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantAccessService, TenantAccessService>();
builder.Services.AddScoped<ITenantRegistrationService, TenantRegistrationService>();
builder.Services.AddScoped<ITenantStoreService, TenantStoreService>();

// ======================================================
// COMMON & INFRASTRUCTURE
// ======================================================
builder.Services.AddScoped<ISlugGenerator, SlugGenerator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ======================================================
// REPOSITORIES (domain‑specific)
// ======================================================
builder.Services.AddScoped<ITenantAccountRepository, TenantAccountRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<ITenantLegalInfoRepository, TenantLegalInfoRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// ======================================================
// BILLING SERVICES
// ======================================================
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISubscriptionRuleEngine, SubscriptionRuleEngine>();
builder.Services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();

// ======================================================
// APPLICATION SERVICES
// ======================================================
builder.Services.AddScoped<TenantApprovalService>();
builder.Services.AddScoped<TenantRegistrationAppService>();
builder.Services.AddScoped<ProductManagementService>();

// ======================================================
// DATABASE
// ======================================================
var provider = builder.Configuration["DatabaseProvider"];
builder.Services.AddDbContext<SaaSPlatformDbContext>(options =>
{
    switch (provider)
    {
        case "Postgres":
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
            break;
        default:
            throw new Exception($"Unsupported database provider: {provider}");
    }
});

// ======================================================
// BUILD & SEED
// ======================================================
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SaaSPlatformDbContext>();
    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ======================================================
// MIDDLEWARE PIPELINE
// ======================================================
app.UseRouting();
app.UseRateLimiter();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SubscriptionMiddleware>();
app.MapControllers();

app.Run();