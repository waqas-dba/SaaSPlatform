using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;

namespace CoreKit.Catalog.Services;

internal static class CatalogGuards
{
    private const int MaxSlugLength = 190;

    /// <summary>
    /// Returns the store if it exists and belongs to the tenant. A store of another
    /// tenant is reported as "not found" so store IDs cannot be probed.
    /// </summary>
    public static async Task<StoreInfo> RequireStoreAsync(
        IStoreInfoProvider provider, Guid tenantId, Guid storeId, CancellationToken ct)
    {
        var store = await provider.GetStoreInfoAsync(storeId, ct);
        if (store is null || store.TenantId != tenantId)
            throw new KeyNotFoundException("Store not found.");

        return store;
    }

    public static async Task EnforceVariantLimitsAsync(
        IPlanLimitProvider? planLimit, Guid tenantId, int variantCount, CancellationToken ct)
    {
        if (planLimit is null) return;

        if (!await planLimit.IsVariantsEnabledAsync(tenantId, ct))
            throw new ForbiddenException("Your plan does not include product variants.");

        var max = await planLimit.GetMaxVariantsPerProductAsync(tenantId, ct);
        if (max.HasValue && variantCount > max.Value)
            throw new InvalidOperationException($"Maximum {max.Value} variants per product allowed.");
    }

    public static async Task EnforceAddonsEnabledAsync(
        IPlanLimitProvider? planLimit, Guid tenantId, CancellationToken ct)
    {
        if (planLimit is null) return;

        if (!await planLimit.IsAddonsEnabledAsync(tenantId, ct))
            throw new ForbiddenException("Your plan does not include add-ons.");
    }

    public static string BaseSlug(string name)
    {
        var slug = SlugHelper.Generate(name);
        return slug.Length > MaxSlugLength ? slug[..MaxSlugLength] : slug;
    }

    /// <summary>Returns baseSlug, or baseSlug-2, baseSlug-3, ... the first one not in <paramref name="taken"/>.</summary>
    public static string PickUniqueSlug(string baseSlug, ISet<string> taken)
    {
        if (!taken.Contains(baseSlug)) return baseSlug;

        for (var i = 2; ; i++)
        {
            var candidate = $"{baseSlug}-{i}";
            if (!taken.Contains(candidate)) return candidate;
        }
    }
}

internal static class CatalogMapper
{
    public static ProductVariantDto ToDto(ProductVariant v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Price = v.Price,
        SortOrder = v.SortOrder,
        IsActive = v.IsActive
    };

    public static ProductImageDto ToDto(ProductImage i) => new()
    {
        Id = i.Id,
        ImageUrl = i.ImageUrl,
        IsPrimary = i.IsPrimary
    };

    public static AddonDto ToDto(Addon a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        AdditionalPrice = a.AdditionalPrice,
        SortOrder = a.SortOrder,
        IsActive = a.IsActive
    };

    public static AddonGroupDto ToDto(AddonGroup g, bool includeInactive) => new()
    {
        Id = g.Id,
        Name = g.Name,
        MinSelect = g.MinSelect,
        MaxSelect = g.MaxSelect,
        SortOrder = g.SortOrder,
        Addons = g.Addons
            .Where(a => includeInactive || a.IsActive)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Name)
            .Select(a => ToDto(a))
            .ToList()
    };

    public static CategoryDto ToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        IconUrl = c.IconUrl,
        SortOrder = c.SortOrder,
        IsActive = c.IsActive,
        ParentCategoryId = c.ParentCategoryId,
        Level = c.Level
    };
}