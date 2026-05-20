using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
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

var connStr = builder.Configuration.GetConnectionString("Postgres")!;
Console.WriteLine($"Connection string: {connStr}");

builder.Services.AddLogging();

builder.Services.AddSingleton<IEncryptionService>(sp =>
    new EncryptionService(builder.Configuration["EncryptionKey"]!));

builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddSingleton(new IamOptions
{
    RunMigrationsOnBootstrap = true,
    AllowDefaultAdminSeed = true,
    SuperAdminRoleName = "SuperAdmin",
    SuperAdminBypassPermissions = true
});

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
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
    logger.LogInformation("Testing database connection...");

    var iamDb = services.GetRequiredService<IamDbContext>();
    var tenantDb = services.GetRequiredService<TenantDbContext>();

    var canConnect = await iamDb.Database.CanConnectAsync();
    if (!canConnect)
    {
        logger.LogCritical("❌ Cannot connect to database.");
        return;
    }
    logger.LogInformation("✔ Database connection successful");

    logger.LogInformation("========================================");
    logger.LogInformation(" APPLYING MIGRATIONS");
    logger.LogInformation("========================================");

    logger.LogInformation("Applying IAM migrations...");
    await iamDb.Database.MigrateAsync();
    logger.LogInformation("✔ IAM migrations applied.");

    logger.LogInformation("Applying Tenant migrations...");
    await tenantDb.Database.MigrateAsync();
    logger.LogInformation("✔ Tenant migrations applied.");

    logger.LogInformation("========================================");
    logger.LogInformation(" CENTRALISED DATABASE BOOTSTRAP");
    logger.LogInformation("========================================");

    var tenantSeeder = services.GetRequiredService<TenantSeeder>();
    await tenantSeeder.SeedAsync();
    logger.LogInformation("✔ Tenant seeding completed.");

    var iamBootstrap = services.GetRequiredService<IamBootstrap>();
    await iamBootstrap.RunAsync();
    logger.LogInformation("✔ IAM bootstrap completed.");

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