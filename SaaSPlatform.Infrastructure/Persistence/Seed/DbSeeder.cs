using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Enums;
using SaaSPlatform.Infrastructure.Services.IAM;

namespace SaaSPlatform.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(SaaSPlatformDbContext db)
    {
        // ======================================================
        // 1. APPLY MIGRATIONS
        // ======================================================
        await db.Database.MigrateAsync();

        // ======================================================
        // 2. SYSTEM TENANT
        // ======================================================
        if (!await db.Tenants.AnyAsync(x => x.Id == SeedData.SystemTenantId))
        {
            await db.Tenants.AddAsync(new Tenant
            {
                Id = SeedData.SystemTenantId,
                Name = "System",
                Slug = "system",
                IsActive = true,
                RegistrationStatus = RegistrationStatus.Approved
            });

            await db.SaveChangesAsync();
        }

        // ======================================================
        // 3. SYSTEM STORE (FIXED DUPLICATE ISSUE)
        // ======================================================
        var storeExists = await db.Stores
            .AnyAsync(x => x.Id == SeedData.SystemStoreId);

        if (!storeExists)
        {
            await db.Stores.AddAsync(new Store
            {
                Id = SeedData.SystemStoreId,
                TenantId = SeedData.SystemTenantId,
                Name = "System Store",
                Slug = "system-store",
                IsOnline = false
            });

            await db.SaveChangesAsync();
        }

        // ======================================================
        // 4. MODULES / PERMISSIONS / PLANS / ROLES
        // ======================================================
        if (!await db.PermissionModules.AnyAsync())
            await db.PermissionModules.AddRangeAsync(SeedData.Modules);

        if (!await db.Permissions.AnyAsync())
            await db.Permissions.AddRangeAsync(SeedData.Permissions);

        if (!await db.Plans.AnyAsync())
            await db.Plans.AddRangeAsync(SeedData.Plans);

        if (!await db.Roles.AnyAsync(x => x.Name == "SuperAdmin"))
            await db.Roles.AddAsync(SeedData.SuperAdminRole);

        await db.SaveChangesAsync();

        // ======================================================
        // 5. ROLE PERMISSIONS (SAFE INSERT)
        // ======================================================
        if (!await db.RolePermissions.AnyAsync())
        {
            var rolePermissions = SeedData.Permissions.Select(p => new RolePermission
            {
                RoleId = SeedData.SuperAdminRoleId,
                PermissionId = p.Id
            });

            await db.RolePermissions.AddRangeAsync(rolePermissions);
            await db.SaveChangesAsync();
        }

        // ======================================================
        // 6. CUISINES / ZONES / ADDONS / GROUPS
        // ======================================================
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

        // ======================================================
        // 7. SUPER ADMIN USER (SAFE)
        // ======================================================
        const string superAdminPhone = "0000000000";

        var userExists = await db.Users.AnyAsync(x => x.Phone == superAdminPhone);

        if (!userExists)
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

            await db.Users.AddAsync(adminUser);
            await db.SaveChangesAsync();

            await db.UserRoles.AddAsync(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = SeedData.SuperAdminRoleId,
                TenantId = SeedData.SystemTenantId
            });

            await db.TenantUsers.AddAsync(new TenantUser
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