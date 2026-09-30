using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Catalog.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IAddonGroupRepository _addonGroupRepo;
    private readonly IStoreProductRepository _storeProductRepo;
    private readonly IStoreInfoProvider _storeInfoProvider;
    private readonly IPlanLimitProvider? _planLimit;

    public ProductService(
        IProductRepository productRepo,
        ICategoryRepository categoryRepo,
        IAddonGroupRepository addonGroupRepo,
        IStoreProductRepository storeProductRepo,
        IStoreInfoProvider storeInfoProvider,
        IPlanLimitProvider? planLimit = null)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _addonGroupRepo = addonGroupRepo;
        _storeProductRepo = storeProductRepo;
        _storeInfoProvider = storeInfoProvider;
        _planLimit = planLimit;
    }

    public async Task<ProductDto> CreateAsync(
        Guid tenantId, CreateProductRequest request, CancellationToken ct = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        ValidateProductFields(name, request.BasePrice, request.PreparationTimeMinutes);
        ValidateVariants(request.Variants);

        var addonGroupIds = request.AddonGroupIds.Distinct().ToList();
        var storeIds = request.StoreIds.Distinct().ToList();

        var category = await _categoryRepo.GetByIdAsync(tenantId, request.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        if (request.Variants.Count > 0)
            await CatalogGuards.EnforceVariantLimitsAsync(_planLimit, tenantId, request.Variants.Count, ct);

        if (addonGroupIds.Count > 0)
        {
            await CatalogGuards.EnforceAddonsEnabledAsync(_planLimit, tenantId, ct);
            await EnsureAddonGroupsExistAsync(tenantId, addonGroupIds, ct);
        }

        foreach (var storeId in storeIds)
            await CatalogGuards.RequireStoreAsync(_storeInfoProvider, tenantId, storeId, ct);

        await _productRepo.BeginTransactionAsync(ct);
        try
        {
            await _productRepo.ExecuteAdvisoryLockAsync(LockKeyHelper.GuidToLockKey(tenantId), ct);
            await EnforceProductCountLimitAsync(tenantId, ct);

            var baseSlug = CatalogGuards.BaseSlug(name);
            var taken = await _productRepo.GetSlugsStartingWithAsync(tenantId, baseSlug, ct);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = category.Id,
                Name = name,
                Slug = CatalogGuards.PickUniqueSlug(baseSlug, taken),
                Description = request.Description?.Trim(),
                BasePrice = request.BasePrice,
                IsVegetarian = request.IsVegetarian,
                PreparationTimeMinutes = request.PreparationTimeMinutes,
                SortOrder = request.SortOrder,
                AvailableForDineIn = request.AvailableForDineIn,
                AvailableForCollection = request.AvailableForCollection,
                AvailableForDelivery = request.AvailableForDelivery
            };

            foreach (var v in request.Variants)
                product.Variants.Add(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Name = v.Name.Trim(),
                    Price = v.Price,
                    SortOrder = v.SortOrder
                });

            var images = request.Images.ToList();
            var primaryIndex = images.FindIndex(i => i.IsPrimary);
            if (primaryIndex < 0 && images.Count > 0) primaryIndex = 0;

            for (var i = 0; i < images.Count; i++)
                product.Images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = images[i].ImageUrl,
                    IsPrimary = i == primaryIndex,
                    SortOrder = i
                });

            for (var i = 0; i < addonGroupIds.Count; i++)
                product.AddonGroupLinks.Add(new ProductAddonGroup
                {
                    Id = Guid.NewGuid(),
                    AddonGroupId = addonGroupIds[i],
                    SortOrder = i
                });

            foreach (var storeId in storeIds)
                product.StoreProducts.Add(new StoreProduct
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    StoreId = storeId,
                    IsAvailable = true
                });

            _productRepo.Add(product);
            await _productRepo.SaveChangesAsync(ct);
            await _productRepo.CommitAsync(ct);

            return await GetByIdAsync(tenantId, product.Id, ct)
                ?? throw new InvalidOperationException("Product saved but could not be retrieved.");
        }
        catch
        {
            await _productRepo.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ProductDto> UpdateAsync(
        Guid tenantId, Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdWithDetailsAsync(tenantId, id, track: true, ct)
            ?? throw new KeyNotFoundException("Product not found.");

        if (request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId)
        {
            var category = await _categoryRepo.GetByIdAsync(tenantId, request.CategoryId.Value, ct)
                ?? throw new KeyNotFoundException("Category not found.");
            product.Category = category;
        }

        if (request.Name is not null)
        {
            var name = request.Name.Trim();
            if (name.Length == 0)
                throw new ArgumentException("Product name cannot be empty.");

            if (!string.Equals(name, product.Name, StringComparison.Ordinal))
            {
                var baseSlug = CatalogGuards.BaseSlug(name);
                var taken = await _productRepo.GetSlugsStartingWithAsync(tenantId, baseSlug, ct);
                taken.Remove(product.Slug);

                product.Name = name;
                product.Slug = CatalogGuards.PickUniqueSlug(baseSlug, taken);
            }
        }

        if (request.Description is not null) product.Description = request.Description.Trim();

        if (request.BasePrice.HasValue)
        {
            if (request.BasePrice.Value < 0)
                throw new ArgumentException("Price cannot be negative.");
            product.BasePrice = request.BasePrice.Value;
        }

        if (request.PreparationTimeMinutes.HasValue)
        {
            ValidatePreparationTime(request.PreparationTimeMinutes);
            product.PreparationTimeMinutes = request.PreparationTimeMinutes;
        }

        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
        if (request.IsVegetarian.HasValue) product.IsVegetarian = request.IsVegetarian.Value;
        if (request.SortOrder.HasValue) product.SortOrder = request.SortOrder.Value;
        if (request.AvailableForDineIn.HasValue) product.AvailableForDineIn = request.AvailableForDineIn.Value;
        if (request.AvailableForCollection.HasValue) product.AvailableForCollection = request.AvailableForCollection.Value;
        if (request.AvailableForDelivery.HasValue) product.AvailableForDelivery = request.AvailableForDelivery.Value;

        if (request.AddonGroupIds is not null)
            await SyncAddonGroupsAsync(product, request.AddonGroupIds, ct);

        if (request.Variants is not null)
            await SyncVariantsAsync(product, request.Variants, ct);

        await _productRepo.SaveChangesAsync(ct);

        return await GetByIdAsync(tenantId, id, ct)
            ?? throw new InvalidOperationException("Product updated but could not be retrieved.");
    }

    public async Task<ProductDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdWithDetailsAsync(tenantId, id, track: false, ct);
        return product is null ? null : MapToDto(product);
    }

    public Task<PagedResult<ProductListDto>> SearchAsync(
        Guid tenantId, ProductFilterQuery filter, CancellationToken ct = default)
    {
        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice)
            throw new ArgumentException("MinPrice cannot be greater than MaxPrice.");

        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);

        return _productRepo.SearchAsync(tenantId, filter, ct);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdWithDetailsAsync(tenantId, id, track: true, ct)
            ?? throw new KeyNotFoundException("Product not found.");

        foreach (var storeProduct in product.StoreProducts.ToList())
            _storeProductRepo.Remove(storeProduct);

        _productRepo.Remove(product);
        await _productRepo.SaveChangesAsync(ct);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private async Task SyncAddonGroupsAsync(Product product, List<Guid> requested, CancellationToken ct)
    {
        var ids = requested.Distinct().ToList();

        if (ids.Count > 0)
        {
            await CatalogGuards.EnforceAddonsEnabledAsync(_planLimit, product.TenantId, ct);
            await EnsureAddonGroupsExistAsync(product.TenantId, ids, ct);
        }

        foreach (var link in product.AddonGroupLinks.Where(l => !ids.Contains(l.AddonGroupId)).ToList())
            _productRepo.RemoveAddonLink(link);

        for (var i = 0; i < ids.Count; i++)
        {
            var existing = product.AddonGroupLinks.FirstOrDefault(l => l.AddonGroupId == ids[i]);
            if (existing is null)
                _productRepo.AddAddonLink(new ProductAddonGroup
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    AddonGroupId = ids[i],
                    SortOrder = i
                });
            else
                existing.SortOrder = i;
        }
    }

    private async Task SyncVariantsAsync(
        Product product, List<CreateVariantRequest> incoming, CancellationToken ct)
    {
        ValidateVariants(incoming);

        if (incoming.Count > 0)
            await CatalogGuards.EnforceVariantLimitsAsync(_planLimit, product.TenantId, incoming.Count, ct);

        var existingByName = product.Variants.ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);
        var incomingNames = incoming.Select(v => v.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var variant in product.Variants.Where(v => !incomingNames.Contains(v.Name)).ToList())
            _productRepo.RemoveVariant(variant);

        foreach (var item in incoming)
        {
            var name = item.Name.Trim();

            if (existingByName.TryGetValue(name, out var existing))
            {
                existing.Price = item.Price;
                existing.SortOrder = item.SortOrder;
            }
            else
            {
                _productRepo.AddVariant(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = name,
                    Price = item.Price,
                    SortOrder = item.SortOrder
                });
            }
        }
    }

    private async Task EnsureAddonGroupsExistAsync(Guid tenantId, List<Guid> ids, CancellationToken ct)
    {
        var count = await _addonGroupRepo.CountExistingAsync(tenantId, ids, ct);
        if (count != ids.Count)
            throw new KeyNotFoundException("One or more add-on groups were not found.");
    }

    private async Task EnforceProductCountLimitAsync(Guid tenantId, CancellationToken ct)
    {
        if (_planLimit is null) return;

        var max = await _planLimit.GetMaxProductsAsync(tenantId, ct);
        if (!max.HasValue) return;

        var count = await _productRepo.CountByTenantAsync(tenantId, ct);
        if (count >= max.Value)
            throw new InvalidOperationException("Product limit reached. Upgrade your plan.");
    }

    private static void ValidateProductFields(string name, decimal basePrice, int? prepMinutes)
    {
        if (name.Length == 0) throw new ArgumentException("Product name is required.");
        if (name.Length > 200) throw new ArgumentException("Product name is too long (max 200).");
        if (basePrice < 0) throw new ArgumentException("Price cannot be negative.");
        ValidatePreparationTime(prepMinutes);
    }

    private static void ValidatePreparationTime(int? minutes)
    {
        if (minutes.HasValue && (minutes.Value < 1 || minutes.Value > 600))
            throw new ArgumentException("Preparation time must be between 1 and 600 minutes.");
    }

    private static void ValidateVariants(IReadOnlyCollection<CreateVariantRequest> variants)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var v in variants)
        {
            var name = v.Name?.Trim() ?? string.Empty;

            if (name.Length == 0) throw new ArgumentException("Variant name is required.");
            if (name.Length > 100) throw new ArgumentException("Variant name is too long (max 100).");
            if (v.Price < 0) throw new ArgumentException($"Variant '{name}' has a negative price.");
            if (!seen.Add(name)) throw new ArgumentException($"Duplicate variant name '{name}'.");
        }
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        TenantId = p.TenantId,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        Name = p.Name,
        Slug = p.Slug,
        Description = p.Description,
        BasePrice = p.BasePrice,
        IsActive = p.IsActive,
        IsVegetarian = p.IsVegetarian,
        PreparationTimeMinutes = p.PreparationTimeMinutes,
        SortOrder = p.SortOrder,
        AvailableForDineIn = p.AvailableForDineIn,
        AvailableForCollection = p.AvailableForCollection,
        AvailableForDelivery = p.AvailableForDelivery,
        Variants = p.Variants
            .OrderBy(v => v.SortOrder).ThenBy(v => v.Name)
            .Select(v => CatalogMapper.ToDto(v))
            .ToList(),
        Images = p.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => CatalogMapper.ToDto(i))
            .ToList(),
        AddonGroups = p.AddonGroupLinks
            .OrderBy(l => l.SortOrder)
            .Select(l => CatalogMapper.ToDto(l.AddonGroup, includeInactive: false))
            .ToList(),
        StoreIds = p.StoreProducts.Select(sp => sp.StoreId).ToList()
    };
}