using AuthCoreKit.IAM.Extensions;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Api.Host.Middleware;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Api.Host.Services;
using SaaSPlatform.Application.Services;
using SaaSPlatform.Application.Validators.Auth;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Core.Billing.Services;
using SaaSPlatform.Core.Catalog.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Persistence.Repositories;
using SaaSPlatform.Infrastructure.Persistence.Seed;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.Billing;
using SaaSPlatform.Infrastructure.Services.Common;
using SaaSPlatform.Infrastructure.Services.Subscriptions;
using SaaSPlatform.SharedKernel.Interfaces;
using TenantKit.Extensions;
using TenantKit.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ... controllers, validation, swagger ...

builder.Services.AddAuthCoreKit<SaaSPlatformDbContext>(
    builder.Configuration.GetSection("Jwt"),
    options => {
        options.LoginIdentifier = "phone";
        // ... other options ...
    });

builder.Services.AddRateLimiter(/* ... */);

// ===== TenantKit =====
builder.Services.AddTenantKit<SaaSPlatformDbContext>(options =>
{
    options.AutoApproveTenants = false;
    options.AllowMultipleStores = true;
    options.EnableLegalInfo = true;
});

builder.Services.AddSingleton<IEncryptionService>(/* ... */);

// Common & Infrastructure
builder.Services.AddScoped<ISlugGenerator, SlugGenerator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Domain-specific repositories (only those not provided by kits)
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Billing services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISubscriptionRuleEngine, SubscriptionRuleEngine>();
builder.Services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();

// Application orchestration services
builder.Services.AddScoped<TenantApprovalService>();
builder.Services.AddScoped<TenantRegistrationAppService>();
builder.Services.AddScoped<ProductManagementService>();
builder.Services.AddScoped<DocumentStorageService>();

// Database
var provider = builder.Configuration["DatabaseProvider"];
builder.Services.AddDbContext<SaaSPlatformDbContext>(options => /* ... */);

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SaaSPlatformDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Middleware pipeline
app.UseRouting();
app.UseRateLimiter();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TenantResolutionMiddleware>();   // from TenantKit
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SubscriptionMiddleware>();      // injects TenantKit.ITenantContext

app.MapControllers();
app.Run();