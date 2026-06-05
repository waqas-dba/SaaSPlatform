// CoreKit.Catalog/Services/ProductService.cs
using System.Text.Json;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Models;
using Microsoft.Extensions.Logging;

namespace CoreKit.Catalog.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IAddonGroupRepository _addonGroupRepo;
    private readonly IVariantValidationService _variantValidator;
    private readonly IStoreInfoProvider _storeInfoProvider;
    private readonly IPlanLimitProvider? _planLimit;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepo,
        ICategoryRepository categoryRepo,
        IAddonGroupRepository addonGroupRepo,
        IVariantValidationService variantValidator,
        IStoreInfoProvider storeInfoProvider,
        ILogger<ProductService> logger,
        IPlanLimitProvider? planLimit = null)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _addonGroupRepo = addonGroupRepo;
        _variantValidator = variantValidator;
        _storeInfoProvider = storeInfoProvider;
        _logger = logger;
        _planLimit = planLimit;

        if (_planLimit is null)
            _logger.LogWarning(
                "IPlanLimitProvider not registered. " +
                "Product, variant and addon limits will not be enforced.");
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken ct = default)
    {
        var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(request.StoreId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        if (category.TenantId != storeInfo.TenantId)
            throw new ForbiddenException("Category does not belong to this tenant.");

        await EnforcePlanLimitsForCreateAsync(request, storeInfo, ct);
        await ValidateAddonGroupAsync(request.AddonGroupId, storeInfo.TenantId, ct);

        if (request.VariantGroupId.HasValue && request.Variants.Any())
            await _variantValidator.ValidateVariantsAgainstGroupAsync(
                request.VariantGroupId.Value, storeInfo.TenantId, request.Variants, ct);

        var lockKey = LockKeyHelper.GuidToLockKey(request.StoreId);
        var tx = await _productRepo.BeginTransactionAsync(ct);
        try
        {
            await _productRepo.ExecuteAdvisoryLockAsync(lockKey, ct);
            await EnforceProductCountLimitAsync(request.StoreId, storeInfo.TenantId, ct);

            var product = await BuildProductAsync(request, storeInfo, ct);
            _productRepo.Add(product);
            await _productRepo.SaveChangesAsync(ct);
            await _productRepo.CommitAsync(ct);

            return await GetByIdAsync(product.Id, ct)
                ?? throw new InvalidOperationException(
                    "Product saved but could not be retrieved.");
        }
        catch
        {
            await _productRepo.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ProductDto> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException("Product not found.");

        var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(product.StoreId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        ApplyScalarUpdates(product, request);
        await ApplyAddonGroupUpdateAsync(product, request, storeInfo, ct);
        ApplyVariantGroupUpdate(product, request);

        if (request.Variants is not null)
            await ApplyVariantUpdatesAsync(product, request.Variants, storeInfo, ct);

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync(ct);

        return await GetByIdAsync(product.Id, ct)
            ?? throw new InvalidOperationException(
                "Product updated but could not be retrieved.");
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdWithDetailsAsync(id, ct);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IReadOnlyList<ProductDto>> GetByStoreAsync(
        Guid storeId, CancellationToken ct = default)
    {
        var products = await _productRepo.GetByStoreAsync(storeId, ct);
        return products.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<ProductDto>> GetByStorePagedAsync(
        Guid storeId, PagedQuery query, CancellationToken ct = default)
    {
        var paged = await _productRepo.GetByStorePagedProjectedAsync(storeId, query, ct);
        var dtos = paged.Items.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            BasePrice = p.BasePrice,
            CategoryName = p.CategoryName,
            IsActive = p.IsActive,
            Images = p.PrimaryImageUrl is null
                ? new()
                : new() { new ProductImageDto { ImageUrl = p.PrimaryImageUrl, IsPrimary = true } }
        }).ToList();

        return PagedResult<ProductDto>.From(
            dtos, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public async Task<PagedResult<ProductListDto>> SearchAsync(
        Guid storeId,
        ProductFilterQuery filter,
        CancellationToken ct = default)
    {
        if (filter.MinPrice.HasValue &&
            filter.MaxPrice.HasValue &&
            filter.MinPrice > filter.MaxPrice)
            throw new ArgumentException("MinPrice cannot be greater than MaxPrice.");

        filter.PageSize = filter.PageSize switch
        {
            < 1 => 1,
            > 100 => 100,
            _ => filter.PageSize
        };
        filter.Page = filter.Page < 1 ? 1 : filter.Page;

        return await _productRepo.SearchAsync(storeId, filter, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Product not found.");
        _productRepo.Remove(product);
        await _productRepo.SaveChangesAsync(ct);
    }

    // ---------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------

    private async Task EnforcePlanLimitsForCreateAsync(
        CreateProductRequest request, StoreInfo storeInfo, CancellationToken ct)
    {
        if (_planLimit is null) return;

        if (request.Variants.Any())
        {
            if (!await _planLimit.IsVariantsEnabledAsync(storeInfo.TenantId, ct))
                throw new ForbiddenException("Your plan does not include product variants.");

            var maxVariants = await _planLimit.GetMaxVariantsPerProductAsync(storeInfo.TenantId, ct);
            if (maxVariants.HasValue && request.Variants.Count > maxVariants.Value)
                throw new InvalidOperationException(
                    $"Maximum {maxVariants.Value} variants per product allowed.");
        }

        if (request.AddonGroupId.HasValue &&
            !await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId, ct))
            throw new ForbiddenException("Your plan does not include add-ons.");
    }

    private async Task EnforceProductCountLimitAsync(
        Guid storeId, Guid tenantId, CancellationToken ct)
    {
        if (_planLimit is null) return;
        var maxProducts = await _planLimit.GetMaxProductsAsync(tenantId, ct);
        if (!maxProducts.HasValue) return;
        var count = await _productRepo.CountByStoreAsync(storeId, ct);
        if (count >= maxProducts.Value)
            throw new InvalidOperationException("Product limit reached. Upgrade your plan.");
    }

    private async Task ValidateAddonGroupAsync(
        Guid? addonGroupId, Guid tenantId, CancellationToken ct)
    {
        if (!addonGroupId.HasValue) return;
        var exists = await _addonGroupRepo.ExistsForTenantAsync(
            addonGroupId.Value, tenantId, ct);
        if (!exists)
            throw new KeyNotFoundException(
                "Addon group not found or does not belong to this tenant.");
    }

    private static void ApplyScalarUpdates(Product product, UpdateProductRequest request)
    {
        if (request.Name is not null)
        {
            product.Name = request.Name;
            product.Slug = SlugHelper.Generate(request.Name);
        }
        if (request.Description is not null)
            product.Description = request.Description;
        if (request.BasePrice.HasValue)
            product.BasePrice = request.BasePrice.Value;
        if (request.IsActive.HasValue)
            product.IsActive = request.IsActive.Value;

        // ── Apply new availability flags ─────────────────────────────────────
        if (request.AvailableForCollection.HasValue)
            product.AvailableForCollection = request.AvailableForCollection.Value;
        if (request.AvailableForDelivery.HasValue)
            product.AvailableForDelivery = request.AvailableForDelivery.Value;
    }

    private async Task ApplyAddonGroupUpdateAsync(
        Product product, UpdateProductRequest request,
        StoreInfo storeInfo, CancellationToken ct)
    {
        if (request.AddonGroupId is null) return;

        if (request.AddonGroupId == Guid.Empty)
        {
            product.AddonGroupId = null;
            return;
        }

        if (_planLimit is not null &&
            !await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId, ct))
            throw new ForbiddenException("Your plan does not include add-ons.");

        await ValidateAddonGroupAsync(request.AddonGroupId, storeInfo.TenantId, ct);
        product.AddonGroupId = request.AddonGroupId.Value;
    }

    private static void ApplyVariantGroupUpdate(
        Product product, UpdateProductRequest request)
    {
        if (!request.VariantGroupId.HasValue) return;
        product.VariantGroupId = request.VariantGroupId == Guid.Empty
            ? null
            : request.VariantGroupId.Value;
    }

    private async Task ApplyVariantUpdatesAsync(
        Product product,
        List<CreateVariantRequest> incoming,
        StoreInfo storeInfo,
        CancellationToken ct)
    {
        if (incoming.Any())
        {
            if (_planLimit is not null)
            {
                if (!await _planLimit.IsVariantsEnabledAsync(storeInfo.TenantId, ct))
                    throw new ForbiddenException("Your plan does not include product variants.");

                var max = await _planLimit.GetMaxVariantsPerProductAsync(storeInfo.TenantId, ct);
                if (max.HasValue && incoming.Count > max.Value)
                    throw new InvalidOperationException(
                        $"Maximum {max.Value} variants per product allowed.");
            }

            if (product.VariantGroupId.HasValue)
                await _variantValidator.ValidateVariantsAgainstGroupAsync(
                    product.VariantGroupId.Value, storeInfo.TenantId, incoming, ct);
        }

        var existingBySku = product.Variants.ToDictionary(v => v.Sku);
        var incomingSkus = incoming.Select(v => v.Sku).ToHashSet();

        foreach (var toRemove in product.Variants
            .Where(v => !incomingSkus.Contains(v.Sku)).ToList())
            product.Variants.Remove(toRemove);

        foreach (var varItem in incoming)
        {
            if (existingBySku.TryGetValue(varItem.Sku, out var existing))
            {
                existing.Price = varItem.Price;
                existing.IsActive = true;
                existing.AttributeValues.Clear();
                await AddVariantAttributesAsync(
                    existing.AttributeValues, varItem.Attributes,
                    storeInfo.StoreTypeCode, ct);
            }
            else
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Sku = varItem.Sku,
                    Price = varItem.Price
                };
                await AddVariantAttributesAsync(
                    variant.AttributeValues, varItem.Attributes,
                    storeInfo.StoreTypeCode, ct);
                product.Variants.Add(variant);
            }
        }
    }

    private async Task AddVariantAttributesAsync(
        ICollection<VariantAttributeValue> target,
        List<VariantAttributeItem> attrs,
        string storeTypeCode,
        CancellationToken ct)
    {
        foreach (var attr in attrs)
        {
            var template = await _variantValidator.ResolveTemplateAsync(
                attr, storeTypeCode, ct);
            target.Add(new VariantAttributeValue
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                Value = attr.Value
            });
        }
    }

    private async Task<Product> BuildProductAsync(
        CreateProductRequest request, StoreInfo storeInfo, CancellationToken ct)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = SlugHelper.Generate(request.Name),
            Description = request.Description,
            BasePrice = request.BasePrice,
            CategoryId = request.CategoryId,
            StoreId = request.StoreId,
            TenantId = storeInfo.TenantId,
            TrackInventory = request.TrackInventory,
            AddonGroupId = request.AddonGroupId,
            VariantGroupId = request.VariantGroupId
        };

        foreach (var attr in request.Attributes)
            product.AttributeValues.Add(new ProductAttributeValue
            {
                Id = Guid.NewGuid(),
                TemplateId = attr.TemplateId,
                Value = attr.Value
            });

        foreach (var varItem in request.Variants)
        {
            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Sku = varItem.Sku,
                Price = varItem.Price
            };
            await AddVariantAttributesAsync(
                variant.AttributeValues, varItem.Attributes,
                storeInfo.StoreTypeCode, ct);
            product.Variants.Add(variant);
        }

        foreach (var img in request.Images)
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = img.ImageUrl,
                IsPrimary = img.IsPrimary
            });

        return product;
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        Description = p.Description,
        BasePrice = p.BasePrice,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        StoreId = p.StoreId,
        TenantId = p.TenantId,
        TrackInventory = p.TrackInventory,
        IsActive = p.IsActive,
        AddonGroupId = p.AddonGroupId,
        VariantGroupId = p.VariantGroupId,
        Images = p.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary
            }).ToList(),
        AttributeValues = p.AttributeValues
            .Select(av => new ProductAttributeValueDto
            {
                Id = av.Id,
                TemplateName = av.Template.Name,
                Value = av.Value
            }).ToList(),
        Variants = p.Variants
            .Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Sku = v.Sku,
                Price = v.Price,
                IsActive = v.IsActive,
                Attributes = v.AttributeValues
                    .Select(va => new VariantAttributeValueDto
                    {
                        Name = va.Template.Name,
                        Value = va.Value
                    }).ToList()
            }).ToList(),
        AddonGroup = p.AddonGroup is null ? null : new AddonGroupDto
        {
            Id = p.AddonGroup.Id,
            Name = p.AddonGroup.Name,
            Addons = p.AddonGroup.Addons
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .Select(a => new AddonDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    AdditionalPrice = a.AdditionalPrice
                }).ToList()
        },
        VariantGroup = p.VariantGroup is null ? null : new VariantGroupDto
        {
            Id = p.VariantGroup.Id,
            Name = p.VariantGroup.Name,
            StoreTypeCode = p.VariantGroup.StoreTypeCode,
            TenantId = p.VariantGroup.TenantId,
            Options = p.VariantGroup.Options
                .OrderBy(o => o.SortOrder)
                .Select(o => new VariantGroupOptionDto
                {
                    Id = o.Id,
                    TemplateId = o.TemplateId,
                    TemplateName = o.Template.Name,
                    AllowedValues = string.IsNullOrWhiteSpace(o.AllowedValuesJson)
                        ? null
                        : JsonSerializer.Deserialize<List<string>>(o.AllowedValuesJson),
                    SortOrder = o.SortOrder
                }).ToList()
        }
    };
}