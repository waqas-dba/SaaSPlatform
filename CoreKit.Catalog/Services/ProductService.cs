// CoreKit.Catalog/Services/ProductService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class ProductService : IProductService
{
    private readonly CatalogDbContext _db;
    private readonly IPlanLimitProvider? _planLimit;

    public ProductService(CatalogDbContext db, IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _planLimit = planLimit;
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var storeInfo = await _db.Set<StoreInfo>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId)
            ?? throw new KeyNotFoundException("Store not found.");

        if (_planLimit != null)
        {
            if (request.Variants.Any())
            {
                if (!await _planLimit.IsVariantsEnabledAsync(storeInfo.TenantId))
                    throw new ForbiddenException("Your plan does not include product variants.");

                var maxVariants = await _planLimit.GetMaxVariantsPerProductAsync(storeInfo.TenantId);
                if (maxVariants.HasValue && request.Variants.Count > maxVariants.Value)
                    throw new InvalidOperationException($"Maximum {maxVariants.Value} variants per product.");
            }

            if (request.Addons.Any())
            {
                if (!await _planLimit.IsAddonsEnabledAsync(storeInfo.TenantId))
                    throw new ForbiddenException("Your plan does not include add-ons.");

                var maxAddons = await _planLimit.GetMaxAddonsPerProductAsync(storeInfo.TenantId);
                if (maxAddons.HasValue && request.Addons.Count > maxAddons.Value)
                    throw new InvalidOperationException($"Maximum {maxAddons.Value} add-ons per product.");
            }

            var maxProducts = await _planLimit.GetMaxProductsAsync(storeInfo.TenantId);
            if (maxProducts.HasValue)
            {
                var currentCount = await _db.Products.CountAsync(p => p.StoreId == storeInfo.Id);
                if (currentCount >= maxProducts.Value)
                    throw new InvalidOperationException("Product limit reached. Upgrade your plan.");
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
                var template = await _db.VariantAttributeTemplates
                    .FirstOrDefaultAsync(t =>
                        t.Name == vAttr.Name &&
                        t.StoreTypeCode == storeInfo.StoreTypeCode)
                    ?? throw new InvalidOperationException(
                        $"Variant attribute '{vAttr.Name}' not defined.");

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
                    // FIX: AddonGroupId is now null for ad-hoc addons instead
                    // of Guid.Empty, which was a sentinel value masking a
                    // schema design issue.
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

        await _db.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues).ThenInclude(av => av.Template)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues).ThenInclude(va => va.Template)
            .Include(p => p.AdHocAddons)
            .Include(p => p.AddonGroup).ThenInclude(ag => ag!.Addons)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : MapToDto(product);
    }

    public async Task<List<ProductDto>> GetByStoreAsync(Guid storeId)
    {
        var products = await _db.Products
            .Where(p => p.StoreId == storeId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants).ThenInclude(v => v.AttributeValues)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
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
}