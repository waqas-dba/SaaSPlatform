using CoreKit.Catalog.Persistence;
using CoreKit.Catalog.Persistence.Seeders;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.IAM.Persistence;
using CoreKit.IAM.Persistence.Seeders;
using CoreKit.IAM.Services;
using CoreKit.Subscription.Persistence;
using CoreKit.Subscription.Persistence.Seeders;
using CoreKit.Tenant.Persistence;
using CoreKit.Tenant.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

var connStr = builder.Configuration.GetConnectionString("Postgres")!;

var encryptionKey = builder.Configuration["EncryptionKey"]
    ?? throw new InvalidOperationException(
        "EncryptionKey is missing from configuration.");

Console.WriteLine("==============================================");
Console.WriteLine("  SaaS PLATFORM DATABASE SEEDER");
Console.WriteLine("==============================================");
Console.WriteLine();

builder.Services.AddLogging();

builder.Services.AddSingleton<IEncryptionService>(sp =>
    new EncryptionService(encryptionKey));

builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddDbContext<SubscriptionDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddSingleton(new IamOptions
{
    RunMigrationsOnBootstrap = true,
    AllowDefaultAdminSeed = true,
    PlatformAdminRoleName = "PlatformAdmin",
    EnableImpersonation = true,
    RequireImpersonationForCrossTenant = true
});

// IAM sub‑seeders
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<PermissionSeeder>();
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<UserSeeder>();
builder.Services.AddScoped<RolePermissionSeeder>();
builder.Services.AddScoped<UserRoleSeeder>();
builder.Services.AddScoped<IamBootstrap>();

// Other module seeders
builder.Services.AddScoped<StoreTypeSeeder>();
builder.Services.AddScoped<TenantSeeder>();
builder.Services.AddScoped<SubscriptionSeeder>();
builder.Services.AddScoped<AttributeTemplateSeeder>();
builder.Services.AddScoped<VariantAttributeTemplateSeeder>();

// ── Optional group seeders (register them even if you don't call them) ──
builder.Services.AddScoped<VariantGroupSeeder>();
builder.Services.AddScoped<AddonGroupSeeder>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Testing database connection...");

    var iamDb = services.GetRequiredService<IamDbContext>();
    var tenantDb = services.GetRequiredService<TenantDbContext>();
    var subDb = services.GetRequiredService<SubscriptionDbContext>();
    var catalogDb = services.GetRequiredService<CatalogDbContext>();

    if (!await iamDb.Database.CanConnectAsync())
    {
        logger.LogCritical(
            "Cannot connect to database. " +
            "Check your connection string and ensure PostgreSQL is running.");
        Console.WriteLine();
        Console.WriteLine("ERROR: Database connection failed.");
        Console.WriteLine($"Connection string: {connStr}");
        return;
    }

    logger.LogInformation("Database connection successful.");

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  APPLYING MIGRATIONS");
    Console.WriteLine("==============================================");

    logger.LogInformation("Applying Tenant migrations...");
    await tenantDb.Database.MigrateAsync();
    logger.LogInformation("Tenant migrations applied successfully.");

    logger.LogInformation("Applying Subscription migrations...");
    await subDb.Database.MigrateAsync();
    logger.LogInformation("Subscription migrations applied successfully.");

    logger.LogInformation("Applying Catalog migrations...");
    await catalogDb.Database.MigrateAsync();
    logger.LogInformation("Catalog migrations applied successfully.");

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  SEEDING STORE TYPES");
    Console.WriteLine("==============================================");

    var storeTypeSeeder = services.GetRequiredService<StoreTypeSeeder>();
    await storeTypeSeeder.SeedAsync();
    logger.LogInformation("Store types seeded successfully.");

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  SEEDING TENANTS");
    Console.WriteLine("==============================================");

    var tenantSeeder = services.GetRequiredService<TenantSeeder>();
    await tenantSeeder.SeedAsync();
    logger.LogInformation("Tenants seeded successfully.");

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  SEEDING SUBSCRIPTION PLANS");
    Console.WriteLine("==============================================");

    var subscriptionSeeder = services.GetRequiredService<SubscriptionSeeder>();
    await subscriptionSeeder.SeedAsync();
    logger.LogInformation("Subscription plans seeded successfully.");

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  IAM BOOTSTRAP (migrations + seeding)");
    Console.WriteLine("==============================================");

    var iamBootstrap = services.GetRequiredService<IamBootstrap>();
    await iamBootstrap.RunAsync();

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  SEEDING CATALOG ATTRIBUTE TEMPLATES");
    Console.WriteLine("==============================================");

    var catalogSeeder = services.GetRequiredService<AttributeTemplateSeeder>();
    await catalogSeeder.SeedAsync();
    logger.LogInformation("Catalog attribute templates seeded successfully.");

    // ── Variant attribute templates ─────────────────────────────────
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  SEEDING VARIANT ATTRIBUTE TEMPLATES");
    Console.WriteLine("==============================================");

    var variantSeeder = services.GetRequiredService<VariantAttributeTemplateSeeder>();
    await variantSeeder.SeedAsync();
    logger.LogInformation("Variant attribute templates seeded successfully.");

//Optional: seed variant groups(currently not used by Postman collection)
     var variantGroupSeeder = services.GetRequiredService<VariantGroupSeeder>();
    await variantGroupSeeder.SeedAsync();
    logger.LogInformation("Variant groups seeded successfully.");

//Optional: seed addon groups(currently not used by Postman collection)
     var addonGroupSeeder = services.GetRequiredService<AddonGroupSeeder>();
    await addonGroupSeeder.SeedAsync();
    logger.LogInformation("Addon groups seeded successfully.");

    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("  DATABASE IS READY");
    Console.WriteLine("==============================================");
    Console.WriteLine();
    Console.WriteLine("Seeded items:");
    Console.WriteLine("  ✓ Store types (10 categories)");
    Console.WriteLine("  ✓ System tenant + system store");
    Console.WriteLine("  ✓ Subscription plans (Free, Basic, Pro)");
    Console.WriteLine("  ✓ Permissions (including platform.*)");
    Console.WriteLine("  ✓ PlatformAdmin role (all permissions)");
    Console.WriteLine("  ✓ Default admin user (phone: 0000000000)");
    Console.WriteLine("  ✓ Catalog attribute templates (restaurant, grocery, ...)");
    Console.WriteLine("  ✓ Variant attribute templates (restaurant, grocery, ...)");
    Console.WriteLine();
    Console.WriteLine("IMPORTANT:");
    Console.WriteLine("  Set 'Seeder:AdminPassword' in your configuration");
    Console.WriteLine("  or a random password was generated.");
    Console.WriteLine("  Rotate it after first login.");
    Console.WriteLine();
    Console.WriteLine("Platform architecture:");
    Console.WriteLine("  - No SuperAdmin bypass");
    Console.WriteLine("  - Permission‑based access control");
    Console.WriteLine("  - Subscription plans ready");
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