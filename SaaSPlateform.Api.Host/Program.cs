using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaSPlatform.Api.Host.Middleware;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Core.Billing.Services;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.IAM.Services;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Persistence.Seed;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.Billing;
using SaaSPlatform.Infrastructure.Services.IAM;
using SaaSPlatform.Infrastructure.Services.TenantServices.Service;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// SERVICES
// ======================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// ======================================================
// JWT (FIXED - SAFE & PRODUCTION READY)
// ======================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);

// SAFE VALIDATION (prevents startup crash)
if (string.IsNullOrWhiteSpace(jwtSettings.Secret) ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new Exception("JWT settings missing or invalid in appsettings.json / appsettings.Development.json");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ======================================================
// RATE LIMITING (.NET 10)
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
// APPLICATION SERVICES
// ======================================================

builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantAccessService, TenantAccessService>();
builder.Services.AddScoped<ITenantRegistrationService, TenantRegistrationService>();
builder.Services.AddScoped<ITenantStoreService, TenantStoreService>();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();
builder.Services.AddScoped<ISubscriptionRuleEngine, SubscriptionRuleEngine>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

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
            throw new Exception($"Unsupported database provider: {provider}");
    }
});

// ======================================================
// BUILD APP
// ======================================================

var app = builder.Build();

// ======================================================
// SEED DATABASE
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SaaSPlatformDbContext>();
    await DbSeeder.SeedAsync(db);
}

// ======================================================
// MIDDLEWARE PIPELINE
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseRateLimiter();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SubscriptionMiddleware>();

app.MapControllers();

app.Run();