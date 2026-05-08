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
        await db.Database.MigrateAsync();

        if (!await db.TenantAccounts.AnyAsync(x => x.Id == SeedData.SystemTenantId))
        {
            await db.TenantAccounts.AddAsync(new TenantAccount
            {
                Id = SeedData.SystemTenantId,
                Name = "System",
                Slug = "system",
                IsActive = true,
                RegistrationStatus = RegistrationStatus.Approved
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Stores.AnyAsync(x => x.Id == SeedData.SystemStoreId))
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

        if (!await db.PermissionModules.AnyAsync()) await db.PermissionModules.AddRangeAsync(SeedData.Modules);
        if (!await db.Permissions.AnyAsync()) await db.Permissions.AddRangeAsync(SeedData.Permissions);
        if (!await db.Plans.AnyAsync()) await db.Plans.AddRangeAsync(SeedData.Plans);
        if (!await db.Roles.AnyAsync(x => x.Name == "SuperAdmin")) await db.Roles.AddAsync(SeedData.SuperAdminRole);
        await db.SaveChangesAsync();

        if (!await db.RolePermissions.AnyAsync())
        {
            var rps = SeedData.Permissions.Select(p => new RolePermission { RoleId = SeedData.SuperAdminRoleId, PermissionId = p.Id });
            await db.RolePermissions.AddRangeAsync(rps);
            await db.SaveChangesAsync();
        }

        if (!await db.Cuisines.AnyAsync()) await db.Cuisines.AddRangeAsync(SeedData.Cuisines);
        if (!await db.Zones.AnyAsync()) await db.Zones.AddRangeAsync(SeedData.Zones);
        if (!await db.Addons.AnyAsync()) await db.Addons.AddRangeAsync(SeedData.Addons);
        if (!await db.AddonGroups.AnyAsync()) await db.AddonGroups.AddRangeAsync(SeedData.AddonGroups);
        if (!await db.AddonGroupItems.AnyAsync()) await db.AddonGroupItems.AddRangeAsync(SeedData.AddonGroupItems);
        await db.SaveChangesAsync();

        const string superAdminPhone = "0000000000";
        if (!await db.Users.AnyAsync(x => x.Phone == superAdminPhone))
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