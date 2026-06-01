// CoreKit.Catalog/Services/ProductVariantService.cs
using CoreKit.Catalog.Abstractions;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class ProductVariantService : IProductVariantService
{
    private readonly CatalogDbContext _db;
    private readonly IStoreInfoProvider _storeInfoProvider;

    // FIX: Inject IStoreInfoProvider instead of querying
    // CoreKit.Tenant.Entities.Store directly from CatalogDbContext.
    // The cross-module dependency is now explicit and swappable.
    public ProductVariantService(
        CatalogDbContext db,
        IStoreInfoProvider storeInfoProvider)
    {
        _db = db;
        _storeInfoProvider = storeInfoProvider;
    }

    public async Task<List<ProductVariantDto>> GetByProductAsync(Guid productId)
    {
        return await _db.ProductVariants
            .Where(v => v.ProductId == productId)
            .Include(v => v.AttributeValues).ThenInclude(av => av.Template)
            .Select(v => MapToDto(v))
            .ToListAsync();
    }

    public async Task<ProductVariantDto?> GetByIdAsync(Guid id)
    {
        var variant = await _db.ProductVariants
            .Include(v => v.AttributeValues).ThenInclude(av => av.Template)
            .FirstOrDefaultAsync(v => v.Id == id);

        return variant is null ? null : MapToDto(variant);
    }

    public async Task<ProductVariantDto> CreateAsync(
        Guid productId,
        CreateVariantRequest request)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new KeyNotFoundException("Product not found.");

        // FIX: Use IStoreInfoProvider instead of _db.Set<Store>()
        var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(product.StoreId)
            ?? throw new KeyNotFoundException("Store not found.");

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Sku = request.Sku,
            Price = request.Price
        };

        foreach (var attr in request.Attributes)
        {
            var template = await _db.VariantAttributeTemplates
                .FirstOrDefaultAsync(t =>
                    t.Name == attr.Name &&
                    t.StoreTypeCode == storeInfo.StoreTypeCode)
                ?? throw new InvalidOperationException(
                    $"Variant attribute '{attr.Name}' not defined.");

            variant.AttributeValues.Add(new VariantAttributeValue
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                Value = attr.Value
            });
        }

        _db.ProductVariants.Add(variant);
        await _db.SaveChangesAsync();

        await _db.Entry(variant)
            .Collection(v => v.AttributeValues).Query()
            .Include(av => av.Template).LoadAsync();

        return MapToDto(variant);
    }

    public async Task UpdateAsync(Guid id, UpdateVariantRequest request)
    {
        var variant = await _db.ProductVariants
            .Include(v => v.AttributeValues)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException("Variant not found.");

        if (request.Sku is not null) variant.Sku = request.Sku;
        if (request.Price.HasValue) variant.Price = request.Price.Value;

        if (request.Attributes is not null)
        {
            // FIX: Remove from DbSet before clearing the in-memory collection.
            // Clearing the collection first discards EF's change tracking
            // references, so the subsequent RemoveRange has nothing to act on.
            // Correct order: remove from DbSet → clear collection → add new.
            _db.VariantAttributeValues.RemoveRange(variant.AttributeValues);
            await _db.SaveChangesAsync(); // flush deletes before re-adding
            variant.AttributeValues.Clear();

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == variant.ProductId)
                ?? throw new KeyNotFoundException("Product not found.");

            // FIX: Use IStoreInfoProvider instead of _db.Set<Store>()
            var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(product.StoreId)
                ?? throw new KeyNotFoundException("Store not found.");

            foreach (var attr in request.Attributes)
            {
                var template = await _db.VariantAttributeTemplates
                    .FirstOrDefaultAsync(t =>
                        t.Name == attr.Name &&
                        t.StoreTypeCode == storeInfo.StoreTypeCode)
                    ?? throw new InvalidOperationException(
                        $"Variant attribute '{attr.Name}' not defined.");

                variant.AttributeValues.Add(new VariantAttributeValue
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Value = attr.Value
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var variant = await _db.ProductVariants.FindAsync(id)
            ?? throw new KeyNotFoundException("Variant not found.");

        _db.ProductVariants.Remove(variant);
        await _db.SaveChangesAsync();
    }

    private static ProductVariantDto MapToDto(ProductVariant variant)
    {
        return new ProductVariantDto
        {
            Id = variant.Id,
            Sku = variant.Sku,
            Price = variant.Price,
            IsActive = variant.IsActive,
            Attributes = variant.AttributeValues.Select(av => new VariantAttributeValueDto
            {
                Name = av.Template.Name,
                Value = av.Value
            }).ToList()
        };
    }
}