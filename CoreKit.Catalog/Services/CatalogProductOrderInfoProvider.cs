// CoreKit.Catalog/Services/CatalogProductOrderInfoProvider.cs
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;          // <-- add this

namespace CoreKit.Catalog.Services;

internal sealed class CatalogProductOrderInfoProvider : IProductOrderInfoProvider
{
    private readonly CatalogDbContext _catalogDb;

    public CatalogProductOrderInfoProvider(CatalogDbContext catalogDb) => _catalogDb = catalogDb;

    public async Task<ProductOrderInfo?> GetProductInfoAsync(Guid productId, CancellationToken ct = default)
    {
        return await _catalogDb.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == productId)
            .Select(x => new ProductOrderInfo
            {
                Id = x.Id,
                StoreId = x.StoreId,
                Name = x.Name,
                BasePrice = x.BasePrice,
                IsActive = !x.IsDeleted && x.IsActive,
                AvailableForCollection = x.AvailableForCollection,
                AvailableForDelivery = x.AvailableForDelivery
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProductVariantOrderInfo?> GetVariantInfoAsync(Guid variantId, CancellationToken ct = default)
    {
        return await _catalogDb.ProductVariants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == variantId)
            .Select(x => new ProductVariantOrderInfo
            {
                Id = x.Id,
                Sku = x.Sku,
                Price = x.Price,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }
}