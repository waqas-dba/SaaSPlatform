using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaSPlatform.Api.Host.Middleware;
using SaaSPlatform.Application.Services;
using SaaSPlatform.Application.Validators.Auth;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Core.Billing.Services;
using SaaSPlatform.Core.Catalog.Interfaces;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.IAM.Services;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Persistence.Repositories;
using SaaSPlatform.Infrastructure.Persistence.Seed;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.Auth;
using SaaSPlatform.Infrastructure.Services.Billing;
using SaaSPlatform.Infrastructure.Services.Common;
using SaaSPlatform.Infrastructure.Services.IAM;
using SaaSPlatform.Infrastructure.Services.Subscriptions;
using SaaSPlatform.Infrastructure.Services.TenantServices;
using SaaSPlatform.SharedKernel.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CORE SERVICES
// ======================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();

// ======================================================
// FLUENT VALIDATION
// ======================================================

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// ======================================================
// JWT CONFIG
// ======================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

if (jwtSettings == null ||
    string.IsNullOrWhiteSpace(jwtSettings.Secret) ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new Exception(
        "Invalid JWT configuration");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,

                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// ======================================================
// RATE LIMITING
// ======================================================

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "default",
        config =>
        {
            config.Window = TimeSpan.FromMinutes(1);

            config.PermitLimit = 100;

            config.QueueLimit = 0;
        });
});

// ======================================================
// TENANT SERVICES
// ======================================================

builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddScoped<ITenantAccessService, TenantAccessService>();

builder.Services.AddScoped<ITenantRegistrationService, TenantRegistrationService>();

builder.Services.AddScoped<ITenantStoreService, TenantStoreService>();

// ======================================================
// COMMON SERVICES
// ======================================================

builder.Services.AddScoped<ISlugGenerator, SlugGenerator>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ======================================================
// REPOSITORIES
// ======================================================

builder.Services.AddScoped<ITenantAccountRepository, TenantAccountRepository>();

builder.Services.AddScoped<IStoreRepository, StoreRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddScoped<ITenantLegalInfoRepository, TenantLegalInfoRepository>();

builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// ======================================================
// IAM SERVICES
// ======================================================

builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IAuthService, AuthService>();

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

            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Postgres"));

            break;

        default:

            throw new Exception(
                $"Unsupported database provider: {provider}");
    }
});

// ======================================================
// BUILD APP
// ======================================================

var app = builder.Build();

// ======================================================
// DATABASE SEED
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<SaaSPlatformDbContext>();

    await DbSeeder.SeedAsync(db);
}

// ======================================================
// DEVELOPMENT
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// ======================================================
// PIPELINE
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