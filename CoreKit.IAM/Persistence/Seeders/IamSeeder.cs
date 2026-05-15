using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreKit.IAM.Persistence.Seeders;

public class IamSeeder
{
    private readonly IamDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<IamSeeder> _logger;

    public IamSeeder(IamDbContext db, IPasswordHasher hasher, ILogger<IamSeeder> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var systemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var catalog = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var orders = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var billing = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var iam = Guid.Parse("30000000-0000-0000-0000-000000000004");

        // ---------- MODULES ----------
        int rows;
        rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'Catalog', 'catalog', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", catalog);
        if (rows == 0) _logger.LogInformation("Module 'Catalog' already exists – skipped.");

        rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'Orders', 'orders', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", orders);
        if (rows == 0) _logger.LogInformation("Module 'Orders' already exists – skipped.");

        rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'Billing', 'billing', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", billing);
        if (rows == 0) _logger.LogInformation("Module 'Billing' already exists – skipped.");

        rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_PermissionModules"" (""Id"", ""Name"", ""Code"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, 'IAM', 'iam', NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_PermissionModules"" WHERE ""Id"" = {0})", iam);
        if (rows == 0) _logger.LogInformation("Module 'IAM' already exists – skipped.");

        // ---------- PERMISSIONS ----------
        await InsertPermissionIfNew("catalog.view", catalog);
        await InsertPermissionIfNew("catalog.create", catalog);
        await InsertPermissionIfNew("catalog.update", catalog);
        await InsertPermissionIfNew("orders.view", orders);
        await InsertPermissionIfNew("orders.manage", orders);
        await InsertPermissionIfNew("billing.view", billing);
        await InsertPermissionIfNew("billing.manage", billing);
        await InsertPermissionIfNew("tenant.view", iam);
        await InsertPermissionIfNew("tenant.approve", iam);
        await InsertPermissionIfNew("store.view", iam);
        await InsertPermissionIfNew("store.update", iam);

        // ---------- SUPER ADMIN ROLE ----------
        var superAdminId = Guid.NewGuid();
        rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_Roles"" (""Id"", ""TenantId"", ""Name"", ""Description"", ""IsSystem"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, NULL, 'SuperAdmin', 'System Super Admin', TRUE, NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_Roles"" WHERE ""Name"" = 'SuperAdmin' AND ""TenantId"" IS NULL)", superAdminId);
        if (rows == 0) _logger.LogInformation("Role 'SuperAdmin' already exists – skipped.");

        var superAdmin = await _db.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == "SuperAdmin" && x.TenantId == null);

        // ---------- DEFAULT SYSTEM USER ----------
        var userId = Guid.NewGuid();
        var passwordHash = _hasher.Hash("Admin@123");
        rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_Users"" (""Id"", ""TenantId"", ""Name"", ""Phone"", ""Email"", ""PasswordHash"", ""IsActive"", ""CreatedAt"", ""IsDeleted"", ""FailedLoginAttempts"") 
              SELECT {0}, NULL, 'System Admin', '0000000000', 'admin@system.com', {1}, TRUE, NOW(), FALSE, 0 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_Users"" WHERE ""Phone"" = '0000000000' OR ""Email"" = 'admin@system.com')", userId, passwordHash);
        if (rows == 0) _logger.LogInformation("User '0000000000' already exists – skipped.");

        // Ensure the password is always correct (even if the user already existed)
        await _db.Database.ExecuteSqlRawAsync(
            @"UPDATE ""IAM_Users"" 
              SET ""PasswordHash"" = {0} 
              WHERE (""Phone"" = '0000000000' OR ""Email"" = 'admin@system.com') 
                AND ""PasswordHash"" != {0}", passwordHash);

        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Phone == "0000000000" || x.Email == "admin@system.com");

        // ---------- USER‑ROLE ASSIGNMENT ----------
        rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_UserRoles"" (""UserId"", ""RoleId"", ""TenantId"") 
              SELECT {0}, {1}, {2} 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_UserRoles"" WHERE ""UserId"" = {0} AND ""RoleId"" = {1} AND ""TenantId"" = {2})",
            user.Id, superAdmin.Id, systemTenantId);
        if (rows == 0) _logger.LogInformation("UserRole assignment already exists – skipped.");
    }

    private async Task InsertPermissionIfNew(string name, Guid moduleId)
    {
        var rows = await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_Permissions"" (""Id"", ""Name"", ""PermissionModuleId"", ""CreatedAt"", ""IsDeleted"") 
              SELECT {0}, {1}, {2}, NOW(), FALSE 
              WHERE NOT EXISTS (SELECT 1 FROM ""IAM_Permissions"" WHERE ""Name"" = {1})",
            Guid.NewGuid(), name, moduleId);
        if (rows == 0) _logger.LogInformation("Permission '{Name}' already exists – skipped.", name);
    }
}