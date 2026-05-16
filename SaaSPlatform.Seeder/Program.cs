using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;
using CoreKit.IAM.Persistence.Seeders;
using CoreKit.IAM.Services;
using CoreKit.Tenant.Persistence;
using CoreKit.Tenant.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// ---------- CONFIGURATION ----------
var connStr = builder.Configuration.GetConnectionString("Postgres")!;
Console.WriteLine($"Connection string: {connStr}");

// ---------- ENCRYPTION ----------
builder.Services.AddSingleton<IEncryptionService>(sp =>
    new EncryptionService(builder.Configuration["EncryptionKey"]!));

// ---------- DB CONTEXTS ----------
builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseNpgsql(connStr));

// ---------- PASSWORD HASHER ----------
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ---------- SEEDERS ----------
builder.Services.AddScoped<PermissionSeeder>();
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<UserSeeder>();
builder.Services.AddScoped<RolePermissionSeeder>();
builder.Services.AddScoped<UserRoleSeeder>();
builder.Services.AddScoped<TenantSeeder>();
builder.Services.AddScoped<IamBootstrap>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    // ---------- QUICK CONNECTION TEST (NO DISPOSE) ----------
    var testDb = services.GetRequiredService<IamDbContext>();
    logger.LogInformation("Testing database connection...");
    var canConnect = await testDb.Database.CanConnectAsync();
    if (!canConnect)
    {
        logger.LogCritical("❌ Cannot connect to database.");
        return;
    }
    logger.LogInformation("✔ Database connection successful");

    logger.LogInformation("========================================");
    logger.LogInformation(" CENTRALISED DATABASE BOOTSTRAP");
    logger.LogInformation("========================================");

    // 1. Tenant data (system tenant + system store)
    var tenantSeeder = services.GetRequiredService<TenantSeeder>();
    await tenantSeeder.SeedAsync();
    logger.LogInformation("Tenant seeding completed.");

    // 2. IAM data (conditional migration, permissions, roles, user, mappings)
    var iamBootstrap = services.GetRequiredService<IamBootstrap>();
    await iamBootstrap.RunAsync();
    logger.LogInformation("IAM bootstrap completed.");

    logger.LogInformation("========================================");
    logger.LogInformation(" DATABASE IS READY FOR USE");
    logger.LogInformation("========================================");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database bootstrap failed");
    Console.WriteLine(ex.ToString());
    throw;
}