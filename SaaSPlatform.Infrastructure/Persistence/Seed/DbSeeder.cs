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

        // ------------------------------------------------------------
        // SYSTEM TENANT
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // SYSTEM STORE (ITenantScoped → IgnoreQueryFilters)
        // ------------------------------------------------------------
        if (!await db.Stores.IgnoreQueryFilters().AnyAsync(x => x.Id == SeedData.SystemStoreId))
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

        // ------------------------------------------------------------
        // MODULES & PERMISSIONS
        // ------------------------------------------------------------
        if (!await db.PermissionModules.AnyAsync())
            await db.PermissionModules.AddRangeAsync(SeedData.Modules);

        if (!await db.Permissions.AnyAsync())
            await db.Permissions.AddRangeAsync(SeedData.Permissions);

        // ------------------------------------------------------------
        // PLANS
        // ------------------------------------------------------------
        if (!await db.Plans.AnyAsync())
            await db.Plans.AddRangeAsync(SeedData.Plans);

        // ------------------------------------------------------------
        // SUPER ADMIN ROLE (ITenantScoped → IgnoreQueryFilters)
        // ------------------------------------------------------------
        if (!await db.Roles.IgnoreQueryFilters().AnyAsync(x => x.Name == "SuperAdmin"))
        {
            await db.Roles.AddAsync(SeedData.SuperAdminRole);
            await db.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        // APPROVER ROLE (ITenantScoped → IgnoreQueryFilters)
        // ------------------------------------------------------------
        if (!await db.Roles.IgnoreQueryFilters().AnyAsync(x => x.Name == "Approver"))
        {
            await db.Roles.AddAsync(SeedData.ApproverRole);
            await db.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        // ROLE-PERMISSIONS for SuperAdmin (all permissions)
        // ------------------------------------------------------------
        if (!await db.RolePermissions.AnyAsync())
        {
            var rps = SeedData.Permissions.Select(p => new RolePermission
            {
                RoleId = SeedData.SuperAdminRoleId,
                PermissionId = p.Id
            });
            await db.RolePermissions.AddRangeAsync(rps);
            await db.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        // ROLE-PERMISSION for Approver (only tenant.approve)
        // ------------------------------------------------------------
        var approvePermId = Guid.Parse("40000000-0000-0000-0000-00000000000B");
        if (!await db.RolePermissions.AnyAsync(rp => rp.RoleId == SeedData.ApproverRoleId && rp.PermissionId == approvePermId))
        {
            await db.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = SeedData.ApproverRoleId,
                PermissionId = approvePermId
            });
            await db.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        // CUISINES & ZONES
        // ------------------------------------------------------------
        if (!await db.Cuisines.AnyAsync())
            await db.Cuisines.AddRangeAsync(SeedData.Cuisines);

        if (!await db.Zones.AnyAsync())
            await db.Zones.AddRangeAsync(SeedData.Zones);

        // ------------------------------------------------------------
        // ADDONS, ADDON GROUPS, ADDON GROUP ITEMS
        // ------------------------------------------------------------
        if (!await db.Addons.IgnoreQueryFilters().AnyAsync())
            await db.Addons.AddRangeAsync(SeedData.Addons);

        if (!await db.AddonGroups.IgnoreQueryFilters().AnyAsync())
            await db.AddonGroups.AddRangeAsync(SeedData.AddonGroups);

        if (!await db.AddonGroupItems.AnyAsync())
            await db.AddonGroupItems.AddRangeAsync(SeedData.AddonGroupItems);

        await db.SaveChangesAsync();

        // ------------------------------------------------------------
        // SUPER ADMIN USER
        // ------------------------------------------------------------
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
                Id = Guid.NewGuid(),
                TenantId = SeedData.SystemTenantId,
                UserId = adminUser.Id,
                IsOwner = false,
                IsActive = true
            });

            await db.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        // APPROVER USER
        // ------------------------------------------------------------
        const string approverPhone = "1111111111";
        if (!await db.Users.AnyAsync(x => x.Phone == approverPhone))
        {
            var approverUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Approver",
                Phone = approverPhone,
                Email = "approver@system.com",
                PasswordHash = new PasswordHasher().Hash("Approver@123"),
                IsActive = true
            };
            await db.Users.AddAsync(approverUser);
            await db.SaveChangesAsync();

            await db.UserRoles.AddAsync(new UserRole
            {
                UserId = approverUser.Id,
                RoleId = SeedData.ApproverRoleId,
                TenantId = SeedData.SystemTenantId
            });

            await db.TenantUsers.AddAsync(new TenantUser
            {
                Id = Guid.NewGuid(),
                TenantId = SeedData.SystemTenantId,
                UserId = approverUser.Id,
                IsOwner = false,
                IsActive = true
            });

            await db.SaveChangesAsync();
        }
    }
}