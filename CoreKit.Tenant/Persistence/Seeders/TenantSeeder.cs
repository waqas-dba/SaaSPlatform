using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Persistence.Seeders;

public class TenantSeeder
{
    private readonly TenantDbContext _db;

    public TenantSeeder(TenantDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        var systemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // ---------------- TENANT ----------------
        if (!await _db.Tenants.AnyAsync(t => t.Id == systemTenantId))
        {
            _db.Tenants.Add(new TenantEntity
            {
                Id = systemTenantId,
                Name = "System",
                Slug = "system",
                Status = TenantStatus.Active
            });

            await _db.SaveChangesAsync();
        }

        // ---------------- STORE ----------------
        var systemStoreId = Guid.Parse("22222222-3333-4444-5555-666666666666");

        if (!await _db.Stores.AnyAsync(s => s.Id == systemStoreId))
        {
            _db.Stores.Add(new Store
            {
                Id = systemStoreId,
                TenantId = systemTenantId,
                Name = "System Store",
                Slug = "system-store",
                IsActive = true
            });

            await _db.SaveChangesAsync();
        }
    }
}