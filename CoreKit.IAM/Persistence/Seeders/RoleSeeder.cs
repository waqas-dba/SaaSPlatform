using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence.Seeders;

public sealed class RoleSeeder
{
    private readonly IamDbContext _db;

    public RoleSeeder(IamDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        var superAdmin = await _db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin" && r.TenantId == null);

        if (superAdmin == null)
        {
            _db.Roles.Add(new Role
            {
                Id = Guid.NewGuid(),
                Name = "SuperAdmin",
                Description = "System Super Administrator",
                TenantId = null,
                IsSystem = true
            });
            await _db.SaveChangesAsync();
        }
    }
}