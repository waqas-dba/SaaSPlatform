using CoreKit.Catalog.Abstractions;
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.Tenant.Abstractions;
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
                "IPlanLimitProvider is not registered. " +
                "Product, variant, and addon limits will not be enforced.");
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken ct = default)
    {
        var storeInfo = await _storeInfoProvider.GetStoreInfoAsync(request.StoreId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        if (_planLimit != null)
        {
            if (request.Variants.Any())
            {
                if (!await _planLimit.IsVariantsEnabledAsync(storeInfo.TenantId, ct))
                    throw new ForbiddenException(
                        "Your plan does not include product variants.");

                var maxVariants = await _planLimit
                    .GetMaxVariantsPerProductAsync(storeInfo.TenantId, ct);

                if (maxVariants.HasValue && request.Variants.Count > maxVariants.Value)
                    throw new InvalidOperationException(
                        $"Maximum {maxVariants.Value} variants per product.");
            }

            if (request.Addons.Any())
            {
                if (!await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId, ct))
                    throw new ForbiddenException(
                        "Your plan does not include add-ons.");

                var maxAddons = await _planLimit
                    .GetMaxAddonsPerProductAsync(storeInfo.TenantId, ct);

                if (maxAddons.HasValue && request.Addons.Count > maxAddons.Value)
                    throw new InvalidOperationException(
                        $"Maximum {maxAddons.Value} add-ons per product.");
            }

            var maxProducts = await _planLimit.GetMaxProductsAsync(storeInfo.TenantId, ct);
            if (maxProducts.HasValue)
            {
                var currentCount = await _db.Products
                    .CountAsync(p => p.StoreId == storeInfo.Id, ct);

                if (currentCount >= maxProducts.Value)
                    throw new InvalidOperationException(
                        "Product limit reached. Upgrade your plan.");
            }
        }

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
            TrackInventory = request.TrackInventory
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

        if (request.AddonGroupId.HasValue)
        {
            product.AddonGroupId = request.AddonGroupId.Value;
        }
        else
        {
            foreach (var addonItem in request.Addons)
            {
                product.AdHocAddons.Add(new Addon
                {
                    Id = Guid.NewGuid(),
                    Name = addonItem.Name,
                    AdditionalPrice = addonItem.AdditionalPrice,
                    AddonGroupId = null
                });
            }
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

        return await GetByIdAsync(product.Id, ct)
            ?? throw new InvalidOperationException(
                "Product was saved but could not be retrieved.");
    }

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
            .Include(p => p.AdHocAddons)
            .Include(p => p.AddonGroup)
                .ThenInclude(ag => ag!.Addons)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return product is null ? null : MapToDto(product);
    }

    // Keep non-ct overload for interface compatibility during transition
    Task<ProductDto?> IProductService.GetByIdAsync(Guid id)
        => GetByIdAsync(id, CancellationToken.None);

    Task<ProductDto> IProductService.CreateAsync(CreateProductRequest request)
        => CreateAsync(request, CancellationToken.None);

    public async Task<List<ProductDto>> GetByStoreAsync(Guid storeId)
    {
        var products = await _db.Products
            .Where(p => p.StoreId == storeId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(va => va.Template)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    private async Task<VariantAttributeTemplate> ResolveVariantTemplateAsync(
        VariantAttributeItem attr,
        string storeTypeCode,
        CancellationToken ct = default)
    {
        if (attr.TemplateId.HasValue)
        {
            return await _db.VariantAttributeTemplates
                .FirstOrDefaultAsync(t => t.Id == attr.TemplateId.Value, ct)
                ?? throw new InvalidOperationException(
                    $"Variant attribute template '{attr.TemplateId}' not found.");
        }

        return await _db.VariantAttributeTemplates
            .FirstOrDefaultAsync(t =>
                t.Name == attr.Name &&
                t.StoreTypeCode == storeTypeCode, ct)
            ?? throw new InvalidOperationException(
                $"Variant attribute '{attr.Name}' not defined " +
                $"for store type '{storeTypeCode}'.");
    }

    private static ProductDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Slug = product.Slug,
        Description = product.Description,
        BasePrice = product.BasePrice,
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.Name ?? string.Empty,
        StoreId = product.StoreId,
        TenantId = product.TenantId,
        TrackInventory = product.TrackInventory,
        IsActive = product.IsActive,
        AddonGroupId = product.AddonGroupId,
        Images = product.Images.Select(i => new ProductImageDto
        {
            Id = i.Id,
            ImageUrl = i.ImageUrl,
            IsPrimary = i.IsPrimary
        }).ToList(),
        AttributeValues = product.AttributeValues.Select(av => new ProductAttributeValueDto
        {
            Id = av.Id,
            TemplateName = av.Template.Name,
            Value = av.Value
        }).ToList(),
        Variants = product.Variants.Select(v => new ProductVariantDto
        {
            Id = v.Id,
            Sku = v.Sku,
            Price = v.Price,
            IsActive = v.IsActive,
            Attributes = v.AttributeValues.Select(va => new VariantAttributeValueDto
            {
                Name = va.Template.Name,
                Value = va.Value
            }).ToList()
        }).ToList(),
        Addons = product.AdHocAddons.Select(a => new AddonDto
        {
            Id = a.Id,
            Name = a.Name,
            AdditionalPrice = a.AdditionalPrice
        }).ToList(),
        AddonGroup = product.AddonGroup is null ? null : new AddonGroupDto
        {
            Id = product.AddonGroup.Id,
            Name = product.AddonGroup.Name,
            Addons = product.AddonGroup.Addons.Select(a => new AddonDto
            {
                Id = a.Id,
                Name = a.Name,
                AdditionalPrice = a.AdditionalPrice
            }).ToList()
        }
    };
}