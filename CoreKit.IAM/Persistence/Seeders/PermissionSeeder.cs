using CoreKit.IAM.Constants;
using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class PermissionSeeder
{
    private readonly IamDbContext _db;

    public PermissionSeeder(IamDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        // Ensure the IAM module exists (idempotent)
        var iamModule = await _db.PermissionModules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Code == "iam");

        if (iamModule == null)
        {
            iamModule = new PermissionModule
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                Name = "IAM",
                Code = "iam"
            };
            _db.PermissionModules.Add(iamModule);
            await _db.SaveChangesAsync();
        }

        // Seed every permission defined in Permissions constants
        foreach (var permissionName in Permissions.All)
        {
            var exists = await _db.Permissions
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Name == permissionName);

            if (exists) continue;

            _db.Permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = permissionName,
                PermissionModuleId = iamModule.Id
            });
        }



        await _db.SaveChangesAsync();
    }
}