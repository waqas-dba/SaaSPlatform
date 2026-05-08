using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(SaaSPlatformDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.PermissionModules.AnyAsync())
            await db.PermissionModules.AddRangeAsync(SeedData.Modules);

        if (!await db.Permissions.AnyAsync())
            await db.Permissions.AddRangeAsync(SeedData.Permissions);

        if (!await db.Plans.AnyAsync())
            await db.Plans.AddRangeAsync(SeedData.Plans);

        if (!await db.Roles.AnyAsync(x => x.Name == "SuperAdmin"))
            await db.Roles.AddAsync(SeedData.SuperAdminRole);

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

        if (!await db.Cuisines.AnyAsync())
            await db.Cuisines.AddRangeAsync(SeedData.Cuisines);

        if (!await db.Zones.AnyAsync())
            await db.Zones.AddRangeAsync(SeedData.Zones);

        if (!await db.Addons.AnyAsync())
            await db.Addons.AddRangeAsync(SeedData.Addons);

        if (!await db.AddonGroups.AnyAsync())
            await db.AddonGroups.AddRangeAsync(SeedData.AddonGroups);

        if (!await db.AddonGroupItems.AnyAsync())
            await db.AddonGroupItems.AddRangeAsync(SeedData.AddonGroupItems);

        await db.SaveChangesAsync();
    }
}