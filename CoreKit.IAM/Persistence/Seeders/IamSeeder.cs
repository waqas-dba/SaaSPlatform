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
        // The system tenant ID must match the one created by TenantSeeder
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
        var existingPermissionNames = await _db.Permissions.Select(p => p.Name).ToListAsync();

        void AddIfNew(string name, Guid moduleId)
        {
            if (!existingPermissionNames.Contains(name))
            {
                _db.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    PermissionModuleId = moduleId
                });
            }
        }

        AddIfNew("catalog.view", catalog);
        AddIfNew("catalog.create", catalog);
        AddIfNew("catalog.update", catalog);
        AddIfNew("orders.view", orders);
        AddIfNew("orders.manage", orders);
        AddIfNew("billing.view", billing);
        AddIfNew("billing.manage", billing);
        AddIfNew("tenant.view", iam);
        AddIfNew("tenant.approve", iam);

        await _db.SaveChangesAsync();

        // ---------- SUPER ADMIN ROLE ----------
        var superAdmin = await _db.Roles.FirstOrDefaultAsync(x => x.Name == "SuperAdmin");
        if (superAdmin == null)
        {
            superAdmin = new Role
            {
                Id = Guid.NewGuid(),
                Name = "SuperAdmin",
                IsSystem = true
            };
            _db.Roles.Add(superAdmin);
            await _db.SaveChangesAsync();
        }

        // ---------- DEFAULT SYSTEM USER ----------
        var userExists = await _db.Users.AnyAsync(x =>
            x.Phone == "0000000000" || x.Email == "admin@system.com");

        if (!userExists)
        {
            var user = new User
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

            // Assign super admin role under the system tenant
            _db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = superAdmin.Id,
                TenantId = systemTenantId    // Required non‑null value
            });
            await _db.SaveChangesAsync();
        }
    }
}