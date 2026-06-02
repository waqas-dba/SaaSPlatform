using CoreKit.Catalog.Abstractions;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreKit.Catalog.Services;

public class AddonService : IAddonService
{
    private readonly CatalogDbContext _db;
    private readonly IStoreInfoProvider _storeInfoProvider;
    private readonly IPlanLimitProvider? _planLimit;
    private readonly ILogger<AddonService> _logger;

    public AddonService(
        CatalogDbContext db,
        IStoreInfoProvider storeInfoProvider,
        ILogger<AddonService> logger,
        IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _storeInfoProvider = storeInfoProvider;
        _logger = logger;
        _planLimit = planLimit;

        if (_planLimit == null)
            _logger.LogWarning(
                "IPlanLimitProvider is not registered. " +
                "Addon limits will not be enforced.");
    }

    public async Task<List<AddonDto>> GetByProductAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        var addons = await _db.Addons
            .Where(a => a.ProductId == productId)
            .ToListAsync(ct);

        return addons.Select(a => new AddonDto
        {
            Id = a.Id,
            Name = a.Name,
            AdditionalPrice = a.AdditionalPrice
        }).ToList();
    }

    public async Task<AddonDto?> GetByIdAsync(
        Guid addonId,
        CancellationToken ct = default)
    {
        var addon = await _db.Addons.FindAsync(
            new object[] { addonId }, ct);

        if (addon == null) return null;

        return new AddonDto
        {
            Id = addon.Id,
            Name = addon.Name,
            AdditionalPrice = addon.AdditionalPrice
        };
    }

    public async Task<AddonDto> CreateAdHocAsync(
        Guid productId,
        string name,
        decimal additionalPrice,
        CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync(
            new object[] { productId }, ct)
            ?? throw new KeyNotFoundException("Product not found.");

        var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(product.StoreId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        if (_planLimit != null)
        {
            if (!await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId, ct))
                throw new ForbiddenException("Your plan does not include add-ons.");

            var maxAddons = await _planLimit.GetMaxAddonsPerProductAsync(
                storeInfo.TenantId, ct);

            if (maxAddons.HasValue)
            {
                var currentCount = await _db.Addons
                    .CountAsync(a => a.ProductId == productId, ct);

                if (currentCount >= maxAddons.Value)
                    throw new InvalidOperationException(
                        $"Maximum {maxAddons.Value} add-ons per product reached. " +
                        "Upgrade your plan.");
            }
        }

        var addon = new Addon
        {
            Id = Guid.NewGuid(),
            Name = name,
            AdditionalPrice = additionalPrice,
            AddonGroupId = null,
            ProductId = productId
        };

        _db.Addons.Add(addon);
        await _db.SaveChangesAsync(ct);

        return new AddonDto
        {
            Id = addon.Id,
            Name = addon.Name,
            AdditionalPrice = addon.AdditionalPrice
        };
    }

    public async Task DeleteAsync(Guid addonId, CancellationToken ct = default)
    {
        var addon = await _db.Addons.FindAsync(
            new object[] { addonId }, ct)
            ?? throw new KeyNotFoundException("Add-on not found.");

        _db.Addons.Remove(addon);
        await _db.SaveChangesAsync(ct);
    }
}