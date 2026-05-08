using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(SaaSPlatformDbContext db)
    {
        // APPLY MIGRATIONS
        await db.Database.MigrateAsync();

        // MODULES
        if (!await db.PermissionModules.AnyAsync())
            await db.PermissionModules.AddRangeAsync(SeedData.Modules);

        // PERMISSIONS
        if (!await db.Permissions.AnyAsync())
            await db.Permissions.AddRangeAsync(SeedData.Permissions);

        // PLANS
        if (!await db.Plans.AnyAsync())
            await db.Plans.AddRangeAsync(SeedData.Plans);

        // SUPER ADMIN ROLE
        if (!await db.Roles.AnyAsync(x => x.Name == "SuperAdmin"))
            await db.Roles.AddAsync(SeedData.SuperAdminRole);

        // ROLE PERMISSIONS
        if (!await db.RolePermissions.AnyAsync())
        {
            var rolePermissions = SeedData.Permissions
                .Select(permission => new RolePermission
                {
                    RoleId = SeedData.SuperAdminRoleId,
                    PermissionId = permission.Id
                });
            await db.RolePermissions.AddRangeAsync(rolePermissions);
        }

        // ===== NEW: CUISINES =====
        if (!await db.Cuisines.AnyAsync())
            await db.Cuisines.AddRangeAsync(SeedData.Cuisines);

        // ===== NEW: ZONES =====
        if (!await db.Zones.AnyAsync())
            await db.Zones.AddRangeAsync(SeedData.Zones);

        // Final save
        await db.SaveChangesAsync();
    }
}