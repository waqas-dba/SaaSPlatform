using CoreKit.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Persistence.Seeders;

public class StoreTypeSeeder
{
    private readonly TenantDbContext _db;

    public StoreTypeSeeder(TenantDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.StoreTypes
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (exists)
            return;

        var items = new List<StoreType>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Restaurant",
                Code = "restaurant",
                Category = "Food",
                Description = "Restaurant and food ordering business",
                IsSystem = true,
                IsActive = true,
                SortOrder = 1
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pharmacy",
                Code = "pharmacy",
                Category = "Healthcare",
                Description = "Medical and pharmacy store",
                IsSystem = true,
                IsActive = true,
                SortOrder = 2
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Hotel",
                Code = "hotel",
                Category = "Hospitality",
                Description = "Hotel and room booking business",
                IsSystem = true,
                IsActive = true,
                SortOrder = 3
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Salon",
                Code = "salon",
                Category = "Beauty",
                Description = "Salon and beauty services",
                IsSystem = true,
                IsActive = true,
                SortOrder = 4
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Grocery",
                Code = "grocery",
                Category = "Retail",
                Description = "Supermarket and grocery business",
                IsSystem = true,
                IsActive = true,
                SortOrder = 5
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Code = "electronics",
                Category = "Retail",
                Description = "Electronics and gadgets store",
                IsSystem = true,
                IsActive = true,
                SortOrder = 6
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Clothing",
                Code = "clothing",
                Category = "Fashion",
                Description = "Fashion and clothing store",
                IsSystem = true,
                IsActive = true,
                SortOrder = 7
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bakery",
                Code = "bakery",
                Category = "Food",
                Description = "Bakery and sweets shop",
                IsSystem = true,
                IsActive = true,
                SortOrder = 8
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Cafe",
                Code = "cafe",
                Category = "Food",
                Description = "Coffee shop and cafe business",
                IsSystem = true,
                IsActive = true,
                SortOrder = 9
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bookstore",
                Code = "bookstore",
                Category = "Retail",
                Description = "Books and stationery business",
                IsSystem = true,
                IsActive = true,
                SortOrder = 10
            }
        };

        await _db.StoreTypes.AddRangeAsync(
            items,
            cancellationToken);

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}