// CoreKit.Catalog/Services/AddonService.cs
using CoreKit.Catalog.Abstractions;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class AddonService : IAddonService
{
    private readonly CatalogDbContext _db;
    private readonly IStoreInfoProvider _storeInfoProvider;
    private readonly IPlanLimitProvider? _planLimit;

    public AddonService(
        CatalogDbContext db,
        IStoreInfoProvider storeInfoProvider,
        IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _storeInfoProvider = storeInfoProvider;
        _planLimit = planLimit;
    }

    public async Task<List<AddonDto>> GetByProductAsync(Guid productId)
    {
        var addons = await _db.Addons
            .Where(a => a.ProductId == productId)
            .ToListAsync();

        return addons.Select(a => new AddonDto
        {
            Id = a.Id,
            Name = a.Name,
            AdditionalPrice = a.AdditionalPrice
        }).ToList();
    }

    public async Task<AddonDto> CreateAdHocAsync(
        Guid productId,
        string name,
        decimal additionalPrice)
    {
        var product = await _db.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(product.StoreId)
            ?? throw new KeyNotFoundException("Store not found.");

        // Enforce plan limits
        if (_planLimit != null)
        {
            if (!await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId))
                throw new ForbiddenException("Your plan does not include add‑ons.");

            var maxAddons = await _planLimit.GetMaxAddonsPerProductAsync(storeInfo.TenantId);
            if (maxAddons.HasValue)
            {
                var currentCount = await _db.Addons.CountAsync(a => a.ProductId == productId);
                if (currentCount >= maxAddons.Value)
                    throw new InvalidOperationException(
                        $"Maximum {maxAddons.Value} add‑ons per product reached. Upgrade your plan.");
            }
        }

        var addon = new Addon
        {
            Id = Guid.NewGuid(),
            Name = name,
            AdditionalPrice = additionalPrice,
            AddonGroupId = null,      // ad‑hoc
            ProductId = productId
        };

        _db.Addons.Add(addon);
        await _db.SaveChangesAsync();

        return new AddonDto
        {
            Id = addon.Id,
            Name = addon.Name,
            AdditionalPrice = addon.AdditionalPrice
        };
    }

    public async Task DeleteAsync(Guid addonId)
    {
        var addon = await _db.Addons.FindAsync(addonId)
            ?? throw new KeyNotFoundException("Add‑on not found.");

        _db.Addons.Remove(addon);
        await _db.SaveChangesAsync();
    }

    public async Task<AddonDto?> GetByIdAsync(Guid addonId)
    {
        var addon = await _db.Addons.FindAsync(addonId);
        if (addon == null) return null;

        return new AddonDto
        {
            Id = addon.Id,
            Name = addon.Name,
            AdditionalPrice = addon.AdditionalPrice
        };
    }
}