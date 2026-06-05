// CoreKit.Catalog/Services/ProductVariantService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class ProductVariantService : IProductVariantService
{
    private readonly CatalogDbContext _db;
    private readonly IStoreInfoProvider _storeInfoProvider;

    public ProductVariantService(
        CatalogDbContext db,
        IStoreInfoProvider storeInfoProvider)
    {
        _db = db;
        _storeInfoProvider = storeInfoProvider;
    }

    public async Task<List<ProductVariantDto>> GetByProductAsync(Guid productId)
        => await _db.ProductVariants
            .Where(v => v.ProductId == productId)
            .Include(v => v.AttributeValues)
                .ThenInclude(av => av.Template)
            .Select(v => MapToDto(v))
            .ToListAsync();

    public async Task<ProductVariantDto?> GetByIdAsync(Guid id)
    {
        var variant = await _db.ProductVariants
            .Include(v => v.AttributeValues)
                .ThenInclude(av => av.Template)
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

        var storeInfo = await _storeInfoProvider
            .GetStoreInfoAsync(product.StoreId)
            ?? throw new KeyNotFoundException("Store not found.");

        // Transaction must wrap the advisory lock so the lock
        // is held until the transaction commits or rolls back.
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var lockKey = LockKeyHelper.GuidToLockKey(product.StoreId);
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0})", lockKey);

            bool skuExists = await _db.ProductVariants
                .AnyAsync(v =>
                    v.Product.StoreId == product.StoreId &&
                    v.Sku == request.Sku);

            if (skuExists)
                throw new InvalidOperationException(
                    $"A variant with SKU '{request.Sku}' already exists in this store.");

            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Sku = request.Sku,
                Price = request.Price
            };

            foreach (var attr in request.Attributes)
            {
                var template = await ResolveVariantTemplateAsync(
                    attr, storeInfo.StoreTypeCode);
                variant.AttributeValues.Add(new VariantAttributeValue
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Value = attr.Value
                });
            }

            _db.ProductVariants.Add(variant);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            await _db.Entry(variant)
                .Collection(v => v.AttributeValues)
                .Query()
                .Include(av => av.Template)
                .LoadAsync();

            return MapToDto(variant);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Guid id, UpdateVariantRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var variant = await _db.ProductVariants
                .Include(v => v.AttributeValues)
                .FirstOrDefaultAsync(v => v.Id == id)
                ?? throw new KeyNotFoundException("Variant not found.");

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == variant.ProductId)
                ?? throw new KeyNotFoundException("Product not found.");

            var storeInfo = await _storeInfoProvider
                .GetStoreInfoAsync(product.StoreId)
                ?? throw new KeyNotFoundException("Store not found.");

            if (request.Sku is not null)
            {
                // Advisory lock must be inside the transaction
                var lockKey = LockKeyHelper.GuidToLockKey(product.StoreId);
                await _db.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock({0})", lockKey);

                bool skuExists = await _db.ProductVariants
                    .AnyAsync(v =>
                        v.Product.StoreId == product.StoreId &&
                        v.Sku == request.Sku &&
                        v.Id != id);

                if (skuExists)
                    throw new InvalidOperationException(
                        $"A variant with SKU '{request.Sku}' already exists in this store.");

                variant.Sku = request.Sku;
            }

            if (request.Price.HasValue)
                variant.Price = request.Price.Value;

            if (request.Attributes is not null)
            {
                if (variant.AttributeValues.Any())
                    _db.VariantAttributeValues.RemoveRange(variant.AttributeValues);

                variant.AttributeValues.Clear();

                foreach (var attr in request.Attributes)
                {
                    var template = await ResolveVariantTemplateAsync(
                        attr, storeInfo.StoreTypeCode);
                    variant.AttributeValues.Add(new VariantAttributeValue
                    {
                        Id = Guid.NewGuid(),
                        TemplateId = template.Id,
                        Value = attr.Value
                    });
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var variant = await _db.ProductVariants.FindAsync(id)
            ?? throw new KeyNotFoundException("Variant not found.");
        _db.ProductVariants.Remove(variant);
        await _db.SaveChangesAsync();
    }

    private async Task<VariantAttributeTemplate> ResolveVariantTemplateAsync(
        VariantAttributeItem attr,
        string storeTypeCode)
    {
        if (attr.TemplateId.HasValue)
            return await _db.VariantAttributeTemplates
                .FirstOrDefaultAsync(t => t.Id == attr.TemplateId.Value)
                ?? throw new InvalidOperationException(
                    $"Variant attribute template '{attr.TemplateId}' not found.");

        return await _db.VariantAttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.Name == attr.Name &&
                t.StoreTypeCode == storeTypeCode)
            ?? throw new InvalidOperationException(
                $"Variant attribute '{attr.Name}' not defined " +
                $"for store type '{storeTypeCode}'.");
    }

    private static ProductVariantDto MapToDto(ProductVariant variant) => new()
    {
        Id = variant.Id,
        Sku = variant.Sku,
        Price = variant.Price,
        IsActive = variant.IsActive,
        Attributes = variant.AttributeValues.Select(av =>
            new VariantAttributeValueDto
            {
                Name = av.Template.Name,
                Value = av.Value
            }).ToList()
    };
}