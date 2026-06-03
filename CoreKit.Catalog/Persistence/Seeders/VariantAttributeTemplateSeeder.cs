using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence.Seeders;

public sealed class VariantAttributeTemplateSeeder
{
    private readonly CatalogDbContext _db;
    public VariantAttributeTemplateSeeder(CatalogDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.VariantAttributeTemplates.AnyAsync(ct))
            return;

        var templates = new List<VariantAttributeTemplate>
        {
            // Restaurant
            new() { Id = Guid.NewGuid(), Name = "Portion Size", StoreTypeCode = "restaurant", OptionsJson = "[\"Small\",\"Medium\",\"Large\"]" },
            // Grocery
            new() { Id = Guid.NewGuid(), Name = "Weight (g)", StoreTypeCode = "grocery", OptionsJson = null },
            // Clothing
            new() { Id = Guid.NewGuid(), Name = "Size", StoreTypeCode = "clothing", OptionsJson = "[\"XS\",\"S\",\"M\",\"L\",\"XL\"]" },
            new() { Id = Guid.NewGuid(), Name = "Color", StoreTypeCode = "clothing", OptionsJson = "[\"Red\",\"Blue\",\"Black\",\"White\"]" },
            // Electronics
            new() { Id = Guid.NewGuid(), Name = "Color", StoreTypeCode = "electronics", OptionsJson = "[\"Black\",\"Silver\",\"White\"]" },
            new() { Id = Guid.NewGuid(), Name = "Storage (GB)", StoreTypeCode = "electronics", OptionsJson = "[\"64\",\"128\",\"256\",\"512\"]" },
            // Bakery
            new() { Id = Guid.NewGuid(), Name = "Size", StoreTypeCode = "bakery", OptionsJson = "[\"Regular\",\"Large\"]" },
            new() { Id = Guid.NewGuid(), Name = "Flavour", StoreTypeCode = "bakery", OptionsJson = "[\"Chocolate\",\"Vanilla\",\"Strawberry\"]" },
            // Cafe
            new() { Id = Guid.NewGuid(), Name = "Size", StoreTypeCode = "cafe", OptionsJson = "[\"Small\",\"Medium\",\"Large\"]" },
            new() { Id = Guid.NewGuid(), Name = "Milk Type", StoreTypeCode = "cafe", OptionsJson = "[\"Full Fat\",\"Skimmed\",\"Oat\",\"Soy\",\"Almond\"]" },
            // Bookstore
            new() { Id = Guid.NewGuid(), Name = "Format", StoreTypeCode = "bookstore", OptionsJson = "[\"Paperback\",\"Hardcover\",\"eBook\"]" },
            // Pharmacy
            new() { Id = Guid.NewGuid(), Name = "Dosage Form", StoreTypeCode = "pharmacy", OptionsJson = "[\"Tablet\",\"Capsule\",\"Syrup\"]" },
            // Hotel
            new() { Id = Guid.NewGuid(), Name = "Room Type", StoreTypeCode = "hotel", OptionsJson = "[\"Single\",\"Double\",\"Suite\"]" },
            // Salon
            new() { Id = Guid.NewGuid(), Name = "Service Type", StoreTypeCode = "salon", OptionsJson = "[\"Basic\",\"Premium\"]" }
        };

        _db.VariantAttributeTemplates.AddRange(templates);
        await _db.SaveChangesAsync(ct);
    }
}