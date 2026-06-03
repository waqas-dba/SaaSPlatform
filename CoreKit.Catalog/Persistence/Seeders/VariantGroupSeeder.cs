// CoreKit.Catalog/Persistence/Seeders/VariantGroupSeeder.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence.Seeders;

public sealed class VariantGroupSeeder
{
    private readonly CatalogDbContext _db;
    public VariantGroupSeeder(CatalogDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.VariantGroups.AnyAsync(ct)) return;

        var sizeTemplateId = await GetTemplateIdAsync("Portion Size", "restaurant", ct);
        if (sizeTemplateId == null) return;

        var group = new VariantGroup
        {
            Id = Guid.NewGuid(),
            Name = "Size & Spice",
            TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StoreTypeCode = "restaurant",
            Options = new List<VariantGroupOption>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    TemplateId = sizeTemplateId.Value,
                    AllowedValuesJson = "[\"Small\",\"Medium\",\"Large\"]",
                    SortOrder = 1
                }
            }
        };

        _db.VariantGroups.Add(group);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Guid?> GetTemplateIdAsync(string name, string storeType, CancellationToken ct)
    {
        return (await _db.VariantAttributeTemplates
            .FirstOrDefaultAsync(t => t.Name == name && t.StoreTypeCode == storeType, ct))?.Id;
    }
}