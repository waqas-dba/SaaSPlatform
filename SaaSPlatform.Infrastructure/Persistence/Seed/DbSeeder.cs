using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Catalog.Entities;
using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Services;
using TenantKit.Entities;
using TenantKit.Enums;

namespace SaaSPlatform.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(SaaSPlatformDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Tenants.AnyAsync(x => x.Id == SeedData.SystemTenantId))
        {
            await db.Tenants.AddAsync(new Tenant
            {
                Id = SeedData.SystemTenantId,
                Name = "System",
                Slug = "system",
                Status = TenantStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Stores.IgnoreQueryFilters().AnyAsync(x => x.Id == SeedData.SystemStoreId))
        {
            await db.Stores.AddAsync(new Store
            {
                Id = SeedData.SystemStoreId,
                TenantId = SeedData.SystemTenantId,
                Name = "System Store",
                Slug = "system-store",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // ... rest unchanged (modules, permissions, plans, roles, cuisines, zones, addons, users) ...
        // ensure all references to TenantAccount removed, uses TenantKit entities.
    }
}