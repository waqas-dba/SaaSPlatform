// CoreKit.Catalog/Services/ProductService.cs

using System.Text.Json;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreKit.Catalog.Services;

public class ProductService : IProductService
{
    private readonly CatalogDbContext _db;
    private readonly IStoreInfoProvider _storeInfoProvider;
    private readonly IPlanLimitProvider? _planLimit;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        CatalogDbContext db,
        IStoreInfoProvider storeInfoProvider,
        ILogger<ProductService> logger,
        IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _storeInfoProvider = storeInfoProvider;
        _logger = logger;
        _planLimit = planLimit;

        if (_planLimit == null)
            _logger.LogWarning(
                "IPlanLimitProvider not registered. " +
                "Product, variant and addon limits will not be enforced.");
    }

    // ── Create ───────────────────────────────────────────────────────

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken ct = default)
    {
        var storeInfo = await _storeInfoProvider
            .GetStoreInfoAsync(request.StoreId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        if (_planLimit != null)
        {
            // Feature + per-request size checks before acquiring lock
            if (request.Variants.Any())
            {
                if (!await _planLimit.IsVariantsEnabledAsync(
                        storeInfo.TenantId, ct))
                    throw new ForbiddenException(
                        "Your plan does not include product variants.");

                var maxVariants = await _planLimit
                    .GetMaxVariantsPerProductAsync(storeInfo.TenantId, ct);

                if (maxVariants.HasValue &&
                    request.Variants.Count > maxVariants.Value)
                    throw new InvalidOperationException(
                        $"Maximum {maxVariants.Value} variants per product " +
                        "allowed by your plan.");
            }

            if (request.AddonGroupId.HasValue &&
                !await _planLimit.IsAddonsEnabledAsync(
                    storeInfo.TenantId, ct))
                throw new ForbiddenException(
                    "Your plan does not include add-ons.");
        }

        // Acquire a lock per store to prevent concurrent product-limit violations
        var lockKey = LockKeyHelper.GuidToLockKey(request.StoreId);
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0})", lockKey);

            if (_planLimit != null)
            {
                var maxProducts = await _planLimit
                    .GetMaxProductsAsync(storeInfo.TenantId, ct);

                if (maxProducts.HasValue)
                {
                    var count = await _db.Products
                        .CountAsync(p => p.StoreId == request.StoreId, ct);

                    if (count >= maxProducts.Value)
                        throw new InvalidOperationException(
                            "Product limit reached. Upgrade your plan.");
                }
            }

            // Validate addon group belongs to this tenant
            if (request.AddonGroupId.HasValue)
                await RequireAddonGroupAsync(
                    request.AddonGroupId.Value, storeInfo.TenantId, ct);

            // Validate variants against variant group (if one is attached)
            if (request.VariantGroupId.HasValue && request.Variants.Any())
                await ValidateVariantsAgainstGroupAsync(
                    request.VariantGroupId.Value,
                    storeInfo.TenantId,
                    request.Variants,
                    ct);

            var product = await BuildAndSaveAsync(request, storeInfo, ct);

            await tx.CommitAsync(ct);

            return await GetByIdAsync(product.Id, ct)
                ?? throw new InvalidOperationException(
                    "Product saved but could not be retrieved.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ── Update ───────────────────────────────────────────────────────

    public async Task<ProductDto> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken ct = default)
    {
        var product = await _db.Products
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException("Product not found.");

        var storeInfo = await _storeInfoProvider
            .GetStoreInfoAsync(product.StoreId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        // Basic field updates
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

        // ── Addon group ─────────────────────────────────────────────
        if (request.AddonGroupId is not null)
        {
            // Guid.Empty means "remove the group"
            if (request.AddonGroupId == Guid.Empty)
            {
                product.AddonGroupId = null;
            }
            else
            {
                if (_planLimit != null &&
                    !await _planLimit.IsAddonsEnabledAsync(
                        storeInfo.TenantId, ct))
                    throw new ForbiddenException(
                        "Your plan does not include add-ons.");

                await RequireAddonGroupAsync(
                    request.AddonGroupId.Value, storeInfo.TenantId, ct);

                product.AddonGroupId = request.AddonGroupId.Value;
            }
        }

        // ── Variant group ────────────────────────────────────────────
        if (request.VariantGroupId.HasValue)
        {
            product.VariantGroupId = request.VariantGroupId == Guid.Empty
                ? null
                : request.VariantGroupId.Value;
        }

        // ── Variants ────────────────────────────────────────────────
        if (request.Variants is not null)
        {
            if (request.Variants.Any())
            {
                if (_planLimit != null)
                {
                    if (!await _planLimit.IsVariantsEnabledAsync(
                            storeInfo.TenantId, ct))
                        throw new ForbiddenException(
                            "Your plan does not include product variants.");

                    var maxVariants = await _planLimit
                        .GetMaxVariantsPerProductAsync(
                            storeInfo.TenantId, ct);

                    if (maxVariants.HasValue &&
                        request.Variants.Count > maxVariants.Value)
                        throw new InvalidOperationException(
                            $"Maximum {maxVariants.Value} variants " +
                            "per product allowed by your plan.");
                }

                // Validate against variant group (if one is attached)
                if (product.VariantGroupId.HasValue)
                    await ValidateVariantsAgainstGroupAsync(
                        product.VariantGroupId.Value,
                        storeInfo.TenantId,
                        request.Variants,
                        ct);
            }

            // Replace all existing variants
            _db.ProductVariants.RemoveRange(product.Variants);
            product.Variants.Clear();

            foreach (var varItem in request.Variants)
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
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

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(product.Id, ct)
            ?? throw new InvalidOperationException(
                "Product updated but could not be retrieved.");
    }

    // ── Read ─────────────────────────────────────────────────────────

    public async Task<ProductDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues)
                .ThenInclude(av => av.Template)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup)
                .ThenInclude(ag => ag!.Addons)
            .Include(p => p.VariantGroup)
                .ThenInclude(vg => vg!.Options)
                    .ThenInclude(o => o.Template)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return product is null ? null : MapToDto(product);
    }

    public async Task<List<ProductDto>> GetByStoreAsync(
        Guid storeId,
        CancellationToken ct = default)
    {
        var products = await _db.Products
            .Where(p => p.StoreId == storeId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues)
                .ThenInclude(av => av.Template)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(va => va.Template)
            .Include(p => p.AddonGroup)
                .ThenInclude(ag => ag!.Addons)
            .Include(p => p.VariantGroup)
                .ThenInclude(vg => vg!.Options)
                    .ThenInclude(o => o.Template)
            .ToListAsync(ct);

        return products.Select(MapToDto).ToList();
    }

    // ── Delete ───────────────────────────────────────────────────────

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products
            .FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Product not found.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
    }

    // ── Internals ────────────────────────────────────────────────────

    private async Task<Product> BuildAndSaveAsync(
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

        _db.Products.Add(product);

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

        foreach (var img in request.Images)
        {
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = img.ImageUrl,
                IsPrimary = img.IsPrimary
            });
        }

        await _db.SaveChangesAsync(ct);
        return product;
    }

    private async Task RequireAddonGroupAsync(
        Guid groupId,
        Guid tenantId,
        CancellationToken ct)
    {
        var exists = await _db.AddonGroups
            .AnyAsync(g =>
                g.Id == groupId &&
                g.TenantId == tenantId, ct);

        if (!exists)
            throw new KeyNotFoundException(
                "Addon group not found or does not belong to this tenant.");
    }

    private async Task<VariantAttributeTemplate> ResolveVariantTemplateAsync(
        VariantAttributeItem attr,
        string storeTypeCode,
        CancellationToken ct)
    {
        if (attr.TemplateId.HasValue)
            return await _db.VariantAttributeTemplates
                .FirstOrDefaultAsync(
                    t => t.Id == attr.TemplateId.Value, ct)
                ?? throw new InvalidOperationException(
                    $"Variant template '{attr.TemplateId}' not found.");

        return await _db.VariantAttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.Name == attr.Name &&
                t.StoreTypeCode == storeTypeCode, ct)
            ?? throw new InvalidOperationException(
                $"Variant attribute '{attr.Name}' not defined " +
                $"for store type '{storeTypeCode}'.");
    }

    private async Task ValidateVariantsAgainstGroupAsync(
        Guid variantGroupId,
        Guid tenantId,
        List<CreateVariantItem> variants,
        CancellationToken ct)
    {
        var group = await _db.VariantGroups
            .Include(g => g.Options)
                .ThenInclude(o => o.Template)
            .FirstOrDefaultAsync(g =>
                g.Id == variantGroupId &&
                g.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                "Variant group not found or does not belong to this tenant.");

        // Build a lookup of templateId → allowed values
        var optionMap = group.Options.ToDictionary(
            o => o.TemplateId,
            o => string.IsNullOrWhiteSpace(o.AllowedValuesJson)
                ? null
                : JsonSerializer.Deserialize<List<string>>(
                    o.AllowedValuesJson));

        foreach (var variant in variants)
        {
            foreach (var attr in variant.Attributes)
            {
                // Resolve template id if only name was provided
                Guid templateId;
                if (attr.TemplateId.HasValue)
                {
                    templateId = attr.TemplateId.Value;
                }
                else
                {
                    var template = await _db.VariantAttributeTemplates
                        .FirstOrDefaultAsync(t =>
                            t.Name == attr.Name &&
                            t.StoreTypeCode == group.StoreTypeCode, ct)
                        ?? throw new InvalidOperationException(
                            $"Variant attribute '{attr.Name}' not found " +
                            $"for store type '{group.StoreTypeCode}'.");
                    templateId = template.Id;
                    attr.TemplateId = templateId;
                }

                if (!optionMap.ContainsKey(templateId))
                    throw new InvalidOperationException(
                        $"Variant attribute template '{templateId}' is not " +
                        "part of the selected variant group.");

                var allowed = optionMap[templateId];
                if (allowed != null && !allowed.Contains(attr.Value))
                    throw new InvalidOperationException(
                        $"Value '{attr.Value}' is not in the allowed list " +
                        $"for this variant option.");
            }
        }
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
                        : JsonSerializer.Deserialize<List<string>>(
                            o.AllowedValuesJson),
                    SortOrder = o.SortOrder
                }).ToList()
        }
    };
}