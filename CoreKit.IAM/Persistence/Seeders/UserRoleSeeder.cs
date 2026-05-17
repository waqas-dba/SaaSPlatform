// CoreKit.IAM | CoreKit.IAM/Persistence/Seeders/UserRoleSeeder.cs
using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class UserRoleSeeder
{
    private readonly IamDbContext _db;

    public UserRoleSeeder(IamDbContext db) => _db = db;

    // CoreKit.IAM/Persistence/Seeders/UserRoleSeeder.cs
    public async Task SeedAsync()
    {
        var adminUser = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                u.Phone == "0000000000" || u.Email == "admin@system.com");

        var superAdmin = await _db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r =>
                r.Name == "SuperAdmin" && r.TenantId == null);

        if (adminUser == null || superAdmin == null) return;

        // PK is now (UserId, RoleId) — check on those two columns only.
        var alreadyAssigned = await _db.UserRoles
            .AnyAsync(ur =>
                ur.UserId == adminUser.Id &&
                ur.RoleId == superAdmin.Id);

        if (alreadyAssigned) return;

        _db.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = superAdmin.Id,
            TenantId = null   // Global SuperAdmin — no tenant
        });

        await _db.SaveChangesAsync();
    }
}