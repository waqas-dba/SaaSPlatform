using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Persistence;          // IamDbContext
using CoreKit.IAM.Services;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using CoreKit.Tenant.Persistence;       // TenantDbContext
using Microsoft.EntityFrameworkCore;

namespace SaaSPlatform.Host.Api.Services;

public class SeedService
{
    private readonly IamDbContext _iamDb;
    private readonly TenantDbContext _tenantDb;
    private readonly IPasswordHasher _passwordHasher;

    // Fixed system IDs
    private static readonly Guid SystemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SystemStoreId = Guid.Parse("22222222-3333-4444-5555-666666666666");

    public SeedService(IamDbContext iamDb, TenantDbContext tenantDb, IPasswordHasher passwordHasher)
    {
        _iamDb = iamDb;
        _tenantDb = tenantDb;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // 1. System tenant
        if (!await _tenantDb.Tenants.AnyAsync(t => t.Id == SystemTenantId))
        {
            _tenantDb.Tenants.Add(new TenantEntity
            {
                Id = SystemTenantId,
                Name = "System",
                Slug = "system",
                Status = TenantStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
            await _tenantDb.SaveChangesAsync();
        }

        // 2. System store
        if (!await _tenantDb.Stores.AnyAsync(s => s.Id == SystemStoreId))
        {
            _tenantDb.Stores.Add(new Store
            {
                Id = SystemStoreId,
                TenantId = SystemTenantId,
                Name = "System Store",
                Slug = "system-store",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _tenantDb.SaveChangesAsync();
        }

        // 3. Permissions (hardcoded list – same as the original SeedData)
        var permissions = new List<Permission>
        {
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), Name = "catalog.view",    PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000001") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000002"), Name = "catalog.create",  PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000001") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000003"), Name = "catalog.update",  PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000001") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000004"), Name = "orders.view",    PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000002") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000005"), Name = "orders.manage",  PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000002") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000006"), Name = "billing.view",   PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000003") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000007"), Name = "billing.manage", PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000003") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000008"), Name = "users.manage",   PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000004") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-000000000009"), Name = "tenant.view",    PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000004") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-00000000000A"), Name = "store.update",   PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000004") },
            new() { Id = Guid.Parse("40000000-0000-0000-0000-00000000000B"), Name = "tenant.approve", PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000004") }
        };

        // Modules (needed for FK)
        if (!await _iamDb.PermissionModules.AnyAsync())
        {
            _iamDb.PermissionModules.AddRange(
                new PermissionModule { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), Name = "Catalog", Code = "catalog" },
                new PermissionModule { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), Name = "Orders", Code = "orders" },
                new PermissionModule { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), Name = "Billing", Code = "billing" },
                new PermissionModule { Id = Guid.Parse("30000000-0000-0000-0000-000000000004"), Name = "IAM", Code = "iam" }
            );
            await _iamDb.SaveChangesAsync();
        }

        if (!await _iamDb.Permissions.AnyAsync())
        {
            _iamDb.Permissions.AddRange(permissions);
            await _iamDb.SaveChangesAsync();
        }

        // 4. Roles
        var superAdminRole = await _iamDb.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin" && r.TenantId == SystemTenantId);
        if (superAdminRole == null)
        {
            superAdminRole = new Role
            {
                Id = Guid.NewGuid(),
                TenantId = SystemTenantId,
                Name = "SuperAdmin",
                Description = "System Super Administrator",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            };
            _iamDb.Roles.Add(superAdminRole);
            await _iamDb.SaveChangesAsync();
        }

        var approverRole = await _iamDb.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == "Approver" && r.TenantId == SystemTenantId);
        if (approverRole == null)
        {
            approverRole = new Role
            {
                Id = Guid.NewGuid(),
                TenantId = SystemTenantId,
                Name = "Approver",
                Description = "Can approve new tenants",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            };
            _iamDb.Roles.Add(approverRole);
            await _iamDb.SaveChangesAsync();
        }

        // Assign all permissions to SuperAdmin
        foreach (var perm in permissions)
        {
            if (!await _iamDb.RolePermissions.AnyAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == perm.Id))
            {
                _iamDb.RolePermissions.Add(new RolePermission { RoleId = superAdminRole.Id, PermissionId = perm.Id });
            }
        }
        // Assign "tenant.approve" to Approver
        var approvePerm = permissions.First(p => p.Name == "tenant.approve");
        if (!await _iamDb.RolePermissions.AnyAsync(rp => rp.RoleId == approverRole.Id && rp.PermissionId == approvePerm.Id))
        {
            _iamDb.RolePermissions.Add(new RolePermission { RoleId = approverRole.Id, PermissionId = approvePerm.Id });
        }
        await _iamDb.SaveChangesAsync();

        // 5. Users
        // SuperAdmin user
        if (!await _iamDb.Users.AnyAsync(u => u.Phone == "0000000000"))
        {
            var superAdminUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = SystemTenantId,
                Name = "Super Admin",
                Phone = "0000000000",
                Email = "admin@system.com",
                PasswordHash = _passwordHasher.Hash("Admin@123"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _iamDb.Users.Add(superAdminUser);
            await _iamDb.SaveChangesAsync();

            _iamDb.UserRoles.Add(new UserRole { UserId = superAdminUser.Id, RoleId = superAdminRole.Id, TenantId = SystemTenantId });
            await _iamDb.SaveChangesAsync();
        }

        // Approver user
        if (!await _iamDb.Users.AnyAsync(u => u.Phone == "1111111111"))
        {
            var approverUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = SystemTenantId,
                Name = "Approver",
                Phone = "1111111111",
                Email = "approver@system.com",
                PasswordHash = _passwordHasher.Hash("Approver@123"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _iamDb.Users.Add(approverUser);
            await _iamDb.SaveChangesAsync();

            _iamDb.UserRoles.Add(new UserRole { UserId = approverUser.Id, RoleId = approverRole.Id, TenantId = SystemTenantId });
            await _iamDb.SaveChangesAsync();
        }
    }
}