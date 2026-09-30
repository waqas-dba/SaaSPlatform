using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence.Seeders;

public sealed class AddonGroupSeeder
{
    private static readonly Guid SystemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly CatalogDbContext _db;

    public AddonGroupSeeder(CatalogDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.AddonGroups.AnyAsync(ct)) return;

        _db.AddonGroups.AddRange(
            new AddonGroup
            {
                Id = Guid.NewGuid(),
                TenantId = SystemTenantId,
                Name = "Pizza Extras",
                MinSelect = 0,
                MaxSelect = 3,
                SortOrder = 1,
                Addons = new List<Addon>
                {
                    new() { Id = Guid.NewGuid(), Name = "Extra Cheese", AdditionalPrice = 150m, SortOrder = 1, IsActive = true },
                    new() { Id = Guid.NewGuid(), Name = "Mushrooms", AdditionalPrice = 100m, SortOrder = 2, IsActive = true },
                    new() { Id = Guid.NewGuid(), Name = "Olives", AdditionalPrice = 80m, SortOrder = 3, IsActive = true }
                }
            },
            new AddonGroup
            {
                Id = Guid.NewGuid(),
                TenantId = SystemTenantId,
                Name = "Choose a Sauce",
                MinSelect = 1,
                MaxSelect = 1,
                SortOrder = 2,
                Addons = new List<Addon>
                {
                    new() { Id = Guid.NewGuid(), Name = "Ketchup", AdditionalPrice = 0m, SortOrder = 1, IsActive = true },
                    new() { Id = Guid.NewGuid(), Name = "Garlic Mayo", AdditionalPrice = 0m, SortOrder = 2, IsActive = true },
                    new() { Id = Guid.NewGuid(), Name = "Hot Sauce", AdditionalPrice = 0m, SortOrder = 3, IsActive = true }
                }
            });

        await _db.SaveChangesAsync(ct);
    }
}