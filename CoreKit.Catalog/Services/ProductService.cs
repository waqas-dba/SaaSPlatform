// FILE: CoreKit.Catalog/Services/ProductService.cs  (full replacement)
// FIX: Made BuildProduct async to eliminate the .GetAwaiter().GetResult()
//      sync-over-async call that can deadlock in some host environments.
//      All callers updated accordingly.

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
    private readonly IVariantGroupRepository _variantGroupRepo;
    private readonly IVariantAttributeTemplateRepository _variantTemplateRepo;
    private readonly IStoreInfoProvider _storeInfoProvider;
    private readonly IPlanLimitProvider? _planLimit;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepo,
        ICategoryRepository categoryRepo,
        IAddonGroupRepository addonGroupRepo,
        IVariantGroupRepository variantGroupRepo,
        IVariantAttributeTemplateRepository variantTemplateRepo,
        IStoreInfoProvider storeInfoProvider,
        ILogger<ProductService> logger,
        IPlanLimitProvider? planLimit = null)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _addonGroupRepo = addonGroupRepo;
        _variantGroupRepo = variantGroupRepo;
        _variantTemplateRepo = variantTemplateRepo;
        _storeInfoProvider = storeInfoProvider;
        _logger = logger;
        _planLimit = planLimit;

        if (_planLimit is null)
            _logger.LogWarning(
                "IPlanLimitProvider not registered. " +
                "Product, variant and addon limits will not be enforced.");
    }

    // -------------------------------------------------------------------------
    // Public interface
    // -------------------------------------------------------------------------

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

        if (_planLimit is not null)
        {
            if (request.Variants.Any())
            {
                if (!await _planLimit.IsVariantsEnabledAsync(storeInfo.TenantId, ct))
                    throw new ForbiddenException(
                        "Your plan does not include product variants.");

                var maxVariants =
                    await _planLimit.GetMaxVariantsPerProductAsync(storeInfo.TenantId, ct);
                if (maxVariants.HasValue && request.Variants.Count > maxVariants.Value)
                    throw new InvalidOperationException(
                        $"Maximum {maxVariants.Value} variants per product allowed by your plan.");
            }

            if (request.AddonGroupId.HasValue &&
                !await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId, ct))
                throw new ForbiddenException("Your plan does not include add-ons.");
        }

        var lockKey = LockKeyHelper.GuidToLockKey(request.StoreId);
        await _productRepo.BeginTransactionAsync(ct);

        try
        {
            await _productRepo.ExecuteAdvisoryLockAsync(lockKey, ct);

            if (_planLimit is not null)
            {
                var maxProducts =
                    await _planLimit.GetMaxProductsAsync(storeInfo.TenantId, ct);
                if (maxProducts.HasValue)
                {
                    var count = await _productRepo.CountByStoreAsync(request.StoreId, ct);
                    if (count >= maxProducts.Value)
                        throw new InvalidOperationException(
                            "Product limit reached. Upgrade your plan.");
                }
            }

            if (request.AddonGroupId.HasValue)
            {
                var addonExists = await _addonGroupRepo.ExistsForTenantAsync(
                    request.AddonGroupId.Value, storeInfo.TenantId, ct);
                if (!addonExists)
                    throw new KeyNotFoundException(
                        "Addon group not found or does not belong to this tenant.");
            }

            if (request.VariantGroupId.HasValue && request.Variants.Any())
                await ValidateVariantsAgainstGroupAsync(
                    request.VariantGroupId.Value,
                    storeInfo.TenantId,
                    request.Variants,
                    ct);

            // FIX: BuildProduct is now async — no more .GetAwaiter().GetResult()
            var product = await BuildProductAsync(request, storeInfo, ct);

            _productRepo.Add(product);
            await _productRepo.SaveChangesAsync(ct);
            await _productRepo.CommitAsync(ct);

            var result = await GetByIdAsync(product.Id, ct);
            return result ?? throw new InvalidOperationException(
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

        if (request.AddonGroupId is not null)
        {
            if (request.AddonGroupId == Guid.Empty)
            {
                product.AddonGroupId = null;
            }
            else
            {
                if (_planLimit is not null &&
                    !await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId, ct))
                    throw new ForbiddenException("Your plan does not include add-ons.");

                var addonExists = await _addonGroupRepo.ExistsForTenantAsync(
                    request.AddonGroupId.Value, storeInfo.TenantId, ct);
                if (!addonExists)
                    throw new KeyNotFoundException(
                        "Addon group not found or does not belong to this tenant.");

                product.AddonGroupId = request.AddonGroupId.Value;
            }
        }

        if (request.VariantGroupId.HasValue)
        {
            product.VariantGroupId = request.VariantGroupId == Guid.Empty
                ? null
                : request.VariantGroupId.Value;
        }

        if (request.Variants is not null)
        {
            if (request.Variants.Any())
            {
                if (_planLimit is not null)
                {
                    if (!await _planLimit.IsVariantsEnabledAsync(storeInfo.TenantId, ct))
                        throw new ForbiddenException(
                            "Your plan does not include product variants.");

                    var maxVariants =
                        await _planLimit.GetMaxVariantsPerProductAsync(storeInfo.TenantId, ct);
                    if (maxVariants.HasValue && request.Variants.Count > maxVariants.Value)
                        throw new InvalidOperationException(
                            $"Maximum {maxVariants.Value} variants per product allowed by your plan.");
                }

                if (product.VariantGroupId.HasValue)
                    await ValidateVariantsAgainstGroupAsync(
                        product.VariantGroupId.Value,
                        storeInfo.TenantId,
                        request.Variants,
                        ct);
            }

            var existingBySku = product.Variants.ToDictionary(v => v.Sku);
            var incomingSkus = request.Variants.Select(v => v.Sku).ToHashSet();

            foreach (var toRemove in product.Variants
                         .Where(v => !incomingSkus.Contains(v.Sku))
                         .ToList())
                product.Variants.Remove(toRemove);

            foreach (var varItem in request.Variants)
            {
                if (existingBySku.TryGetValue(varItem.Sku, out var existing))
                {
                    existing.Price = varItem.Price;
                    existing.IsActive = true;
                    existing.AttributeValues.Clear();

                    foreach (var vAttr in varItem.Attributes)
                    {
                        var template = await ResolveVariantTemplateAsync(
                            vAttr, storeInfo.StoreTypeCode, ct);
                        existing.AttributeValues.Add(new VariantAttributeValue
                        {
                            Id = Guid.NewGuid(),
                            TemplateId = template.Id,
                            Value = vAttr.Value
                        });
                    }
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

                    foreach (var vAttr in varItem.Attributes)
                    {
                        var template = await ResolveVariantTemplateAsync(
                            vAttr, storeInfo.StoreTypeCode, ct);
                        variant.AttributeValues.Add(new VariantAttributeValue
                        {
                            Id = Guid.NewGuid(),
                            TemplateId = template.Id,
                            Value = vAttr.Value
                        });
                    }

                    product.Variants.Add(variant);
                }
            }
        }

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync(ct);

        var result = await GetByIdAsync(product.Id, ct);
        return result ?? throw new InvalidOperationException(
            "Product updated but could not be retrieved.");
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdWithDetailsAsync(id, ct);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IReadOnlyList<ProductDto>> GetByStoreAsync(
        Guid storeId,
        CancellationToken ct = default)
    {
        var products = await _productRepo.GetByStoreAsync(storeId, ct);
        return products.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Product not found.");

        _productRepo.Remove(product);
        await _productRepo.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a new <see cref="Product"/> from the request.
    /// Previously this was synchronous and called async helpers via
    /// .GetAwaiter().GetResult(), risking deadlocks. Now fully async.
    /// </summary>
    private async Task<Product> BuildProductAsync(
        CreateProductRequest request,
        StoreInfo storeInfo,
        CancellationToken ct)
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
        {
            product.AttributeValues.Add(new ProductAttributeValue
            {
                Id = Guid.NewGuid(),
                TemplateId = attr.TemplateId,
                Value = attr.Value
            });
        }

        foreach (var varItem in request.Variants)
        {
            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Sku = varItem.Sku,
                Price = varItem.Price
            };

            foreach (var vAttr in varItem.Attributes)
            {
                // FIX: was .GetAwaiter().GetResult() — now properly awaited
                var template = await ResolveVariantTemplateAsync(
                    vAttr, storeInfo.StoreTypeCode, ct);

                variant.AttributeValues.Add(new VariantAttributeValue
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Value = vAttr.Value
                });
            }

            product.Variants.Add(variant);
        }

        foreach (var img in request.Images)
        {
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = img.ImageUrl,
                IsPrimary = img.IsPrimary
            });
        }

        return product;
    }

    private async Task<VariantAttributeTemplate> ResolveVariantTemplateAsync(
        VariantAttributeItem attr,
        string storeTypeCode,
        CancellationToken ct)
    {
        if (attr.TemplateId.HasValue)
            return await _variantTemplateRepo.GetByIdAsync(attr.TemplateId.Value, ct)
                   ?? throw new InvalidOperationException(
                       $"Variant template '{attr.TemplateId}' not found.");

        return await _variantTemplateRepo.GetByNameAndStoreTypeAsync(
                   attr.Name, storeTypeCode, ct)
               ?? throw new InvalidOperationException(
                   $"Variant attribute '{attr.Name}' not defined " +
                   $"for store type '{storeTypeCode}'.");
    }

    private async Task ValidateVariantsAgainstGroupAsync(
        Guid variantGroupId,
        Guid tenantId,
        List<CreateVariantRequest> variants,
        CancellationToken ct)
    {
        var group = await _variantGroupRepo.GetByIdWithOptionsAsync(
                        variantGroupId, tenantId, ct)
                    ?? throw new KeyNotFoundException(
                        "Variant group not found or does not belong to this tenant.");

        var optionMap = group.Options.ToDictionary(
            o => o.TemplateId,
            o => string.IsNullOrWhiteSpace(o.AllowedValuesJson)
                ? null
                : JsonSerializer.Deserialize<List<string>>(o.AllowedValuesJson));

        foreach (var variant in variants)
        {
            foreach (var attr in variant.Attributes)
            {
                Guid templateId;

                if (attr.TemplateId.HasValue)
                {
                    templateId = attr.TemplateId.Value;
                }
                else
                {
                    var template = await _variantTemplateRepo
                                       .GetByNameAndStoreTypeAsync(
                                           attr.Name, group.StoreTypeCode, ct)
                                   ?? throw new InvalidOperationException(
                                       $"Variant attribute '{attr.Name}' not found " +
                                       $"for store type '{group.StoreTypeCode}'.");

                    templateId = template.Id;
                    attr.TemplateId = templateId;
                }

                if (!optionMap.ContainsKey(templateId))
                    throw new InvalidOperationException(
                        $"Variant attribute template '{templateId}' is not part " +
                        "of the selected variant group.");

                var allowed = optionMap[templateId];
                if (allowed != null && !allowed.Contains(attr.Value))
                    throw new InvalidOperationException(
                        $"Value '{attr.Value}' is not in the allowed list " +
                        "for this variant option.");
            }
        }
    }



    // ─────────────────────────────────────────────────────────────────────────────
    // FILE: CoreKit.Catalog/Services/ProductService.cs  — ADD this method
    //       (paste inside the ProductService class, after GetByStoreAsync)
    // ─────────────────────────────────────────────────────────────────────────────

    
    public async Task<PagedResult<ProductDto>> GetByStorePagedAsync(
        Guid storeId,
        PagedQuery query,
        CancellationToken ct = default)
    {
        var pagedProducts = await _productRepo.GetByStorePagedAsync(storeId, query, ct);

        var dtos = pagedProducts.Items.Select(MapToDto).ToList();

        return PagedResult<ProductDto>.From(
            dtos,
            pagedProducts.TotalCount,
            pagedProducts.Page,
            pagedProducts.PageSize);
    }
    

    // -------------------------------------------------------------------------
    // Mapping
    // -------------------------------------------------------------------------

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
        AddonGroup = p.AddonGroup is null
            ? null
            : new AddonGroupDto
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
        VariantGroup = p.VariantGroup is null
            ? null
            : new VariantGroupDto
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