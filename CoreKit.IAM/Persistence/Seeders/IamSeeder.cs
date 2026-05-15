using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public class IamSeeder
{
    private readonly IamDbContext _db;
    private readonly IPasswordHasher _hasher;

    public IamSeeder(IamDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task SeedAsync()
    {
        var systemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var catalog = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var orders = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var billing = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var iam = Guid.Parse("30000000-0000-0000-0000-000000000004");

        // ---------- MODULES ----------
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'Catalog', 'catalog', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", catalog);
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'Orders', 'orders', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", orders);
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'Billing', 'billing', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", billing);
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'IAM', 'iam', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", iam);

        // ---------- PERMISSIONS ----------
        await InsertPermissionIfNotExists("catalog.view", catalog);
        await InsertPermissionIfNotExists("catalog.create", catalog);
        await InsertPermissionIfNotExists("catalog.update", catalog);
        await InsertPermissionIfNotExists("orders.view", orders);
        await InsertPermissionIfNotExists("orders.manage", orders);
        await InsertPermissionIfNotExists("billing.view", billing);
        await InsertPermissionIfNotExists("billing.manage", billing);
        await InsertPermissionIfNotExists("tenant.view", iam);
        await InsertPermissionIfNotExists("tenant.approve", iam);

        // ---------- SUPER ADMIN ROLE ----------
        var superAdminId = Guid.NewGuid();
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_Roles"" (""Id"", ""TenantId"", ""Name"", ""Description"", ""IsSystem"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, NULL, 'SuperAdmin', 'System Super Admin', TRUE, NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_Roles"" WHERE ""Name"" = 'SuperAdmin' AND ""TenantId"" IS NULL)", superAdminId);

        // Retrieve the role (could be existing)
        var superAdmin = await _db.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == "SuperAdmin" && x.TenantId == null);

        // ---------- DEFAULT SYSTEM USER ----------
        var userId = Guid.NewGuid();
        var passwordHash = _hasher.Hash("Admin@123");
        await _db.Database.ExecuteSqlRawAsync(
     @"INSERT INTO ""IAM_Users"" (""Id"", ""TenantId"", ""Name"", ""Phone"", ""Email"", ""PasswordHash"", ""IsActive"", ""CreatedAt"", ""IsDeleted"", ""FailedLoginAttempts"") 
      SELECT {0}, NULL, 'System Admin', '0000000000', 'admin@system.com', {1}, TRUE, NOW(), FALSE, 0 
      WHERE NOT EXISTS (SELECT 1 FROM ""IAM_Users"" WHERE ""Phone"" = '0000000000' OR ""Email"" = 'admin@system.com')", userId, passwordHash);

        // Retrieve the user (could be existing)
        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Phone == "0000000000" || x.Email == "admin@system.com");

        // ---------- USER‑ROLE ASSIGNMENT ----------
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_UserRoles"" (""UserId"", ""RoleId"", ""TenantId"") 
              SELECT {0}, {1}, {2} 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_UserRoles"" WHERE ""UserId"" = {0} AND ""RoleId"" = {1} AND ""TenantId"" = {2})",
            user.Id, superAdmin.Id, systemTenantId);
    }

    private async Task InsertPermissionIfNotExists(string name, Guid moduleId)
    {
        var id = Guid.NewGuid();
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_Permissions"" (""Id"", ""Name"", ""PermissionModuleId"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, {1}, {2}, NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_Permissions"" WHERE ""Name"" = {1})", id, name, moduleId);
    }
}