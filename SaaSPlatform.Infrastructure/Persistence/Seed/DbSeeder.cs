using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Enums;
using SaaSPlatform.Infrastructure.Services.IAM;   // for PasswordHasher

namespace SaaSPlatform.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(SaaSPlatformDbContext db)
    {
        // 1. Apply pending migrations
        await db.Database.MigrateAsync();

        // 2. System tenant & store (must exist before any tenant‑scoped entity)
        if (!await db.Tenants.AnyAsync(t => t.Id == SeedData.SystemTenantId))
        {
            db.Tenants.Add(new Tenant
            {
                Id = SeedData.SystemTenantId,
                Name = "System",
                Slug = "system",
                IsActive = true,
                RegistrationStatus = RegistrationStatus.Approved
            });
        }

        if (!await db.Stores.AnyAsync(s => s.Id == SeedData.SystemStoreId))
        {
            db.Stores.Add(new Store
            {
                Id = SeedData.SystemStoreId,
                TenantId = SeedData.SystemTenantId,
                Name = "System Store",
                Slug = "system-store",
                IsOnline = false
            });
        }

        await db.SaveChangesAsync();

        // 3. Modules, permissions, plans, super admin role
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

        // 4. Cuisines, zones, addons, addon groups
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

        // 5. Super admin user (depends on system tenant & super admin role)
        const string superAdminPhone = "0000000000";
        if (!await db.Users.AnyAsync(u => u.Phone == superAdminPhone))
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Super Admin",
                Phone = superAdminPhone,
                Email = "admin@system.com",
                PasswordHash = new PasswordHasher().Hash("Admin@123"),
                IsActive = true
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = SeedData.SuperAdminRoleId,
                TenantId = SeedData.SystemTenantId
            });

            db.TenantUsers.Add(new TenantUser
            {
                TenantId = SeedData.SystemTenantId,
                UserId = adminUser.Id,
                IsOwner = false,
                IsActive = true
            });

            await db.SaveChangesAsync();
        }
    }
}