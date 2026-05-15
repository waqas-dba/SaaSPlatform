using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

/// <summary>
/// Seeds the minimum required IAM data: modules, permissions, SuperAdmin role,
/// and a built‑in system admin user (0000000000 / Admin@123).
/// This seeder is idempotent – it can run multiple times without errors.
/// </summary>
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
        // System tenant – must match the one created by TenantSeeder
        var systemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var catalog = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var orders = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var billing = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var iam = Guid.Parse("30000000-0000-0000-0000-000000000004");

        // ---------- MODULES ----------
        if (!await _db.PermissionModules.AnyAsync(x => x.Id == catalog))
            _db.PermissionModules.Add(new PermissionModule { Id = catalog, Name = "Catalog", Code = "catalog" });

        if (!await _db.PermissionModules.AnyAsync(x => x.Id == orders))
            _db.PermissionModules.Add(new PermissionModule { Id = orders, Name = "Orders", Code = "orders" });

        if (!await _db.PermissionModules.AnyAsync(x => x.Id == billing))
            _db.PermissionModules.Add(new PermissionModule { Id = billing, Name = "Billing", Code = "billing" });

        if (!await _db.PermissionModules.AnyAsync(x => x.Id == iam))
            _db.PermissionModules.Add(new PermissionModule { Id = iam, Name = "IAM", Code = "iam" });

        await _db.SaveChangesAsync();

        // ---------- PERMISSIONS ----------
        var existingPermissions = await _db.Permissions.Select(p => p.Name).ToListAsync();

        void AddPermissionIfNew(string name, Guid moduleId)
        {
            if (!existingPermissions.Contains(name))
            {
                _db.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    PermissionModuleId = moduleId
                });
            }
        }

        AddPermissionIfNew("catalog.view", catalog);
        AddPermissionIfNew("catalog.create", catalog);
        AddPermissionIfNew("catalog.update", catalog);
        AddPermissionIfNew("orders.view", orders);
        AddPermissionIfNew("orders.manage", orders);
        AddPermissionIfNew("billing.view", billing);
        AddPermissionIfNew("billing.manage", billing);
        AddPermissionIfNew("tenant.view", iam);
        AddPermissionIfNew("tenant.approve", iam);

        await _db.SaveChangesAsync();

        // ---------- SUPER ADMIN ROLE ----------
        var superAdmin = await _db.Roles.FirstOrDefaultAsync(x => x.Name == "SuperAdmin");
        if (superAdmin == null)
        {
            superAdmin = new Role
            {
                Id = Guid.NewGuid(),
                Name = "SuperAdmin",
                Description = "System Super Admin",
                IsSystem = true
            };
            _db.Roles.Add(superAdmin);
            await _db.SaveChangesAsync();
        }

        // ---------- DEFAULT SYSTEM USER ----------
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Phone == "0000000000" || x.Email == "admin@system.com");
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = "System Admin",
                Phone = "0000000000",
                Email = "admin@system.com",
                PasswordHash = _hasher.Hash("Admin@123"),
                IsActive = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        // ---------- USER‑ROLE ASSIGNMENT (idempotent using PostgreSQL ON CONFLICT) ----------
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""IAM_UserRoles"" (""UserId"", ""RoleId"", ""TenantId"") 
              VALUES ({0}, {1}, {2}) 
              ON CONFLICT DO NOTHING",
            user.Id,
            superAdmin.Id,
            systemTenantId);
    }
}