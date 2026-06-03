// CoreKit.Catalog/Persistence/Seeders/AddonGroupSeeder.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence.Seeders;

public sealed class AddonGroupSeeder
{
    private readonly CatalogDbContext _db;
    public AddonGroupSeeder(CatalogDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.AddonGroups.AnyAsync(ct)) return;

        var group = new AddonGroup
        {
            Id = Guid.NewGuid(),
            Name = "Pizza Extras",
            TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Addons = new List<Addon>
            {
                new() { Id = Guid.NewGuid(), Name = "Extra Cheese", AdditionalPrice = 1.50m, IsActive = true },
                new() { Id = Guid.NewGuid(), Name = "Mushrooms", AdditionalPrice = 0.75m, IsActive = true },
                new() { Id = Guid.NewGuid(), Name = "Olives", AdditionalPrice = 0.50m, IsActive = true }
            }
        };

        _db.AddonGroups.Add(group);
        await _db.SaveChangesAsync(ct);
    }
}