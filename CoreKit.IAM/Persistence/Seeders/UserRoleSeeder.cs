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

        var platformAdmin = await _db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r =>
                r.Name == "PlatformAdmin" && r.TenantId == null);

        if (adminUser == null || platformAdmin == null) return;

        var alreadyAssigned = await _db.UserRoles
            .AnyAsync(ur =>
                ur.UserId == adminUser.Id &&
                ur.RoleId == platformAdmin.Id);

        if (alreadyAssigned) return;

        _db.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = platformAdmin.Id,
            TenantId = null
        });

        await _db.SaveChangesAsync();
    }
}