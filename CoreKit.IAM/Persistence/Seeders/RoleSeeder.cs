using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class RoleSeeder
{
    private readonly IamDbContext _db;

    public RoleSeeder(IamDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        var platformAdmin = await _db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == "PlatformAdmin" && r.TenantId == null);

        if (platformAdmin == null)
        {
            _db.Roles.Add(new Role
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Name = "PlatformAdmin",
                Description = "Platform administrator with full platform permissions",
                TenantId = null,
                IsSystem = true
            });

            await _db.SaveChangesAsync();
        }
    }
}