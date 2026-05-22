using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class RolePermissionSeeder
{
    private readonly IamDbContext _db;

    public RolePermissionSeeder(IamDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        var platformAdmin = await _db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == "PlatformAdmin" && r.TenantId == null);

        if (platformAdmin == null) return;

        var allPermissions = await _db.Permissions.ToListAsync();

        foreach (var permission in allPermissions)
        {
            var exists = await _db.RolePermissions
                .AnyAsync(rp => rp.RoleId == platformAdmin.Id && rp.PermissionId == permission.Id);

            if (exists) continue;

            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = platformAdmin.Id,
                PermissionId = permission.Id
            });
        }

        await _db.SaveChangesAsync();
    }
}