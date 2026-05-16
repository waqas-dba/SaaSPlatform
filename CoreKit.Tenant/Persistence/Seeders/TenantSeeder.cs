using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreKit.Tenant.Persistence.Seeders;

public sealed class TenantSeeder
{
    private readonly TenantDbContext _db;
    private readonly ILogger<TenantSeeder> _logger;

    public TenantSeeder(TenantDbContext db, ILogger<TenantSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Tenant seeding started...");

        var systemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var systemStoreId = Guid.Parse("22222222-3333-4444-5555-666666666666");

        // ---------- SYSTEM TENANT ----------
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
            _logger.LogInformation("✔ System tenant created.");
        }
        else
        {
            _logger.LogInformation("System tenant already exists.");
        }

        // ---------- SYSTEM STORE ----------
        if (!await _db.Stores.AnyAsync(s => s.Id == systemStoreId))
        {
            _db.Stores.Add(new Store
            {
                Id = systemStoreId,
                TenantId = systemTenantId,
                Name = "System Store",
                Slug = "system-store",
                IsActive = true,
                IsPrimary = true
            });
            await _db.SaveChangesAsync();
            _logger.LogInformation("✔ System store created.");
        }
        else
        {
            _logger.LogInformation("System store already exists.");
        }

        _logger.LogInformation("Tenant seeding completed.");
    }
}