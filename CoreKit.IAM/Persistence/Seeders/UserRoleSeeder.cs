// CoreKit.IAM | CoreKit.IAM/Persistence/Seeders/UserRoleSeeder.cs
using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class UserRoleSeeder
{
    private readonly IamDbContext _db;

    public UserRoleSeeder(IamDbContext db) => _db = db;

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

        // FIX: SuperAdmin is a global role — TenantId = null.
        // No fake system tenant GUID needed now that UserRole.TenantId is nullable.
        var alreadyAssigned = await _db.UserRoles
            .AnyAsync(ur =>
                ur.UserId == adminUser.Id &&
                ur.RoleId == superAdmin.Id &&
                ur.TenantId == null);

        if (alreadyAssigned) return;

        _db.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = superAdmin.Id,
            TenantId = null   // global scope
        });

        await _db.SaveChangesAsync();
    }
}