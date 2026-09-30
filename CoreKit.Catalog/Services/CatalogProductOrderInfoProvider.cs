using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

/// <summary>
/// Answers the Order module's questions. Reads bypass the request's tenant filter
/// because anonymous customers place orders without a resolved tenant, so every
/// query is keyed by IDs and by the store instead.
/// </summary>
internal sealed class CatalogProductOrderInfoProvider : IProductOrderInfoProvider
{
    private readonly CatalogDbContext _db;

    public CatalogProductOrderInfoProvider(CatalogDbContext db) => _db = db;

    public Task<ProductOrderInfo?> GetProductInfoAsync(
        Guid productId, Guid storeId, CancellationToken ct = default)
        => _db.StoreProducts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(sp => sp.ProductId == productId && sp.StoreId == storeId)
            .Select(sp => new ProductOrderInfo
            {
                Id = sp.ProductId,
                TenantId = sp.TenantId,
                StoreId = sp.StoreId,
                Name = sp.Product.Name,
                UnitPrice = sp.PriceOverride ?? sp.Product.BasePrice,
                IsActive = sp.IsAvailable && sp.Product.IsActive && !sp.Product.IsDeleted,
                HasVariants = sp.Product.Variants.Any(v => v.IsActive),
                AvailableForDineIn = sp.Product.AvailableForDineIn,
                AvailableForCollection = sp.Product.AvailableForCollection,
                AvailableForDelivery = sp.Product.AvailableForDelivery
            })
            .FirstOrDefaultAsync(ct);

    public Task<ProductVariantOrderInfo?> GetVariantInfoAsync(
        Guid variantId, Guid productId, CancellationToken ct = default)
        => _db.ProductVariants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(v => v.Id == variantId && v.ProductId == productId)
            .Select(v => new ProductVariantOrderInfo
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Name = v.Name,
                Price = v.Price,
                IsActive = v.IsActive
            })
            .FirstOrDefaultAsync(ct);

    public async Task<AddonSelectionResult> ResolveAddonsAsync(
        Guid productId, IReadOnlyCollection<Guid> addonIds, CancellationToken ct = default)
    {
        var errors = new List<string>();
        var requested = addonIds.Distinct().ToList();

        if (requested.Count != addonIds.Count)
            errors.Add("Duplicate add-ons are not allowed.");

        var groups = await _db.ProductAddonGroups
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.ProductId == productId && !l.AddonGroup.IsDeleted)
            .Select(l => new
            {
                GroupId = l.AddonGroupId,
                GroupName = l.AddonGroup.Name,
                l.AddonGroup.MinSelect,
                l.AddonGroup.MaxSelect,
                Addons = l.AddonGroup.Addons
                    .Where(a => a.IsActive)
                    .Select(a => new { a.Id, a.Name, a.AdditionalPrice })
                    .ToList()
            })
            .ToListAsync(ct);

        var lookup = new Dictionary<Guid, AddonOrderInfo>();
        foreach (var group in groups)
            foreach (var addon in group.Addons)
                lookup[addon.Id] = new AddonOrderInfo
                {
                    Id = addon.Id,
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    Name = addon.Name,
                    AdditionalPrice = addon.AdditionalPrice
                };

        var resolved = new List<AddonOrderInfo>();
        foreach (var id in requested)
        {
            if (lookup.TryGetValue(id, out var info))
                resolved.Add(info);
            else
                errors.Add($"Add-on '{id}' is not available for this product.");
        }

        foreach (var group in groups)
        {
            var selected = resolved.Count(r => r.GroupId == group.GroupId);

            if (selected < group.MinSelect)
                errors.Add($"Choose at least {group.MinSelect} option(s) for '{group.GroupName}'.");

            if (selected > group.MaxSelect)
                errors.Add($"Choose at most {group.MaxSelect} option(s) for '{group.GroupName}'.");
        }

        return new AddonSelectionResult { Addons = resolved, Errors = errors };
    }
}