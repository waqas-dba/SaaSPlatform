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

// ==========================================
// CONFIGURATION
// ==========================================
var connStr = builder.Configuration.GetConnectionString("Postgres")!;
var encryptionKey = builder.Configuration["EncryptionKey"]
    ?? throw new InvalidOperationException(
        "EncryptionKey is missing from configuration.");

Console.WriteLine("==============================================");
Console.WriteLine("  SaaS PLATFORM DATABASE SEEDER");
Console.WriteLine("==============================================");
Console.WriteLine();

// ==========================================
// LOGGING
// ==========================================
builder.Services.AddLogging();

// ==========================================
// ENCRYPTION SERVICE
// ==========================================
builder.Services.AddSingleton<IEncryptionService>(sp =>
    new EncryptionService(encryptionKey));

// ==========================================
// DATABASE CONTEXTS
// ==========================================
builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseNpgsql(connStr));

// ==========================================
// IAM OPTIONS — NO BYPASS
// ==========================================
builder.Services.AddSingleton(new IamOptions
{
    RunMigrationsOnBootstrap = true,
    AllowDefaultAdminSeed = true,
    PlatformAdminRoleName = "PlatformAdmin",
    EnableImpersonation = true,
    RequireImpersonationForCrossTenant = true
});

// ==========================================
// SERVICES FOR SEEDING
// ==========================================
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ==========================================
// SEEDERS
// ==========================================
builder.Services.AddScoped<PermissionSeeder>();
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<UserSeeder>();
builder.Services.AddScoped<RolePermissionSeeder>();
builder.Services.AddScoped<UserRoleSeeder>();
builder.Services.AddScoped<StoreTypeSeeder>();
builder.Services.AddScoped<TenantSeeder>();
builder.Services.AddScoped<IamBootstrap>();

// ==========================================
// BUILD HOST
// ==========================================
var app = builder.Build();

// ==========================================
// EXECUTE SEEDING
// ==========================================
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    // ==========================================
    // STEP 1: TEST CONNECTION
    // ==========================================
    logger.LogInformation("Testing database connection...");

    var iamDb = services.GetRequiredService<IamDbContext>();
    var tenantDb = services.GetRequiredService<TenantDbContext>();

    var canConnect = await iamDb.Database.CanConnectAsync();
    if (!canConnect)
    {
        logger.LogCritical("Cannot connect to database. Check your connection string and ensure PostgreSQL is running.");
        Console.WriteLine();
        Console.WriteLine("ERROR: Database connection failed.");
        Console.WriteLine($"Connection string: {connStr}");
        return;
    }

    logger.LogInformation("Database connection successful.");

    // ==========================================
    // STEP 2: APPLY MIGRATIONS
    // ==========================================
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  APPLYING MIGRATIONS");
    Console.WriteLine("==============================================");

    logger.LogInformation("Applying IAM migrations...");
    await iamDb.Database.MigrateAsync();
    logger.LogInformation("IAM migrations applied successfully.");

    logger.LogInformation("Applying Tenant migrations...");
    await tenantDb.Database.MigrateAsync();
    logger.LogInformation("Tenant migrations applied successfully.");

    // ==========================================
    // STEP 3: SEED STORE TYPES FIRST
    // (Tenants/Stores depend on StoreTypes)
    // ==========================================
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  SEEDING STORE TYPES");
    Console.WriteLine("==============================================");

    var storeTypeSeeder = services.GetRequiredService<StoreTypeSeeder>();
    await storeTypeSeeder.SeedAsync();
    logger.LogInformation("Store types seeded successfully.");

    // ==========================================
    // STEP 4: SEED TENANTS (after StoreTypes)
    // ==========================================
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  SEEDING TENANTS");
    Console.WriteLine("==============================================");

    var tenantSeeder = services.GetRequiredService<TenantSeeder>();
    await tenantSeeder.SeedAsync();
    logger.LogInformation("Tenants seeded successfully.");

    // ==========================================
    // STEP 5: IAM BOOTSTRAP
    // ==========================================
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  IAM BOOTSTRAP");
    Console.WriteLine("==============================================");

    logger.LogInformation("Seeding permissions...");
    var permissionSeeder = services.GetRequiredService<PermissionSeeder>();
    await permissionSeeder.SeedAsync();
    logger.LogInformation("Permissions seeded successfully.");

    logger.LogInformation("Seeding roles (PlatformAdmin)...");
    var roleSeeder = services.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedAsync();
    logger.LogInformation("Roles seeded successfully.");

    logger.LogInformation("Seeding default admin user...");
    var userSeeder = services.GetRequiredService<UserSeeder>();
    await userSeeder.SeedAsync();
    logger.LogInformation("Default admin user seeded successfully.");

    logger.LogInformation("Assigning permissions to PlatformAdmin role...");
    var rolePermissionSeeder = services.GetRequiredService<RolePermissionSeeder>();
    await rolePermissionSeeder.SeedAsync();
    logger.LogInformation("Role permissions assigned successfully.");

    logger.LogInformation("Assigning PlatformAdmin role to default user...");
    var userRoleSeeder = services.GetRequiredService<UserRoleSeeder>();
    await userRoleSeeder.SeedAsync();
    logger.LogInformation("User role assigned successfully.");

    // ==========================================
    // SUMMARY
    // ==========================================
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  DATABASE IS READY");
    Console.WriteLine("==============================================");
    Console.WriteLine();
    Console.WriteLine("Seeded items:");
    Console.WriteLine("  ✓ Store types (10 categories)");
    Console.WriteLine("  ✓ System tenant + system store");
    Console.WriteLine("  ✓ Permissions (including platform.*)");
    Console.WriteLine("  ✓ PlatformAdmin role (all permissions)");
    Console.WriteLine("  ✓ Default admin user (phone: 0000000000)");
    Console.WriteLine();
    Console.WriteLine("IMPORTANT:");
    Console.WriteLine("  Set 'Seeder:AdminPassword' in your configuration");
    Console.WriteLine("  or a random password was generated.");
    Console.WriteLine("  Rotate it after first login.");
    Console.WriteLine();
    Console.WriteLine("Platform architecture:");
    Console.WriteLine("  - No SuperAdmin bypass");
    Console.WriteLine("  - Permission-based access control");
    Console.WriteLine("  - Use /api/admin/impersonation for tenant access");
    Console.WriteLine();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database bootstrap failed: {Message}", ex.Message);

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  BOOTSTRAP FAILED");
    Console.WriteLine("==============================================");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine();

    if (ex.InnerException != null)
        Console.WriteLine($"Inner: {ex.InnerException.Message}");

    Console.WriteLine();
    throw;
}