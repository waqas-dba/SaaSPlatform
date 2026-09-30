using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Interfaces;

namespace CoreKit.Catalog.Services;

public sealed class ProductVariantService : IProductVariantService
{
    private readonly IProductRepository _productRepo;
    private readonly IPlanLimitProvider? _planLimit;

    public ProductVariantService(IProductRepository productRepo, IPlanLimitProvider? planLimit = null)
    {
        _productRepo = productRepo;
        _planLimit = planLimit;
    }

    public async Task<List<ProductVariantDto>> GetByProductAsync(
        Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, track: false, ct);

        return product.Variants
            .OrderBy(v => v.SortOrder).ThenBy(v => v.Name)
            .Select(v => CatalogMapper.ToDto(v))
            .ToList();
    }

    public async Task<ProductVariantDto?> GetByIdAsync(
        Guid tenantId, Guid productId, Guid variantId, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, track: false, ct);
        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        return variant is null ? null : CatalogMapper.ToDto(variant);
    }

    public async Task<ProductVariantDto> CreateAsync(
        Guid tenantId, Guid productId, CreateVariantRequest request, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, track: true, ct);

        var name = ValidateName(request.Name);
        ValidatePrice(request.Price);
        EnsureNameFree(product, name, exceptId: null);

        await CatalogGuards.EnforceVariantLimitsAsync(_planLimit, tenantId, product.Variants.Count + 1, ct);

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = name,
            Price = request.Price,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        _productRepo.AddVariant(variant);
        await _productRepo.SaveChangesAsync(ct);

        return CatalogMapper.ToDto(variant);
    }

    public async Task<ProductVariantDto> UpdateAsync(
        Guid tenantId, Guid productId, Guid variantId, UpdateVariantRequest request,
        CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, track: true, ct);
        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new KeyNotFoundException("Variant not found.");

        if (request.Name is not null)
        {
            var name = ValidateName(request.Name);
            EnsureNameFree(product, name, exceptId: variant.Id);
            variant.Name = name;
        }

        if (request.Price.HasValue)
        {
            ValidatePrice(request.Price.Value);
            variant.Price = request.Price.Value;
        }

        if (request.SortOrder.HasValue) variant.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) variant.IsActive = request.IsActive.Value;

        await _productRepo.SaveChangesAsync(ct);
        return CatalogMapper.ToDto(variant);
    }

    public async Task DeleteAsync(
        Guid tenantId, Guid productId, Guid variantId, CancellationToken ct = default)
    {
        var product = await LoadAsync(tenantId, productId, track: true, ct);
        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new KeyNotFoundException("Variant not found.");

        _productRepo.RemoveVariant(variant);
        await _productRepo.SaveChangesAsync(ct);
    }

    private async Task<Product> LoadAsync(Guid tenantId, Guid productId, bool track, CancellationToken ct)
        => await _productRepo.GetByIdWithVariantsAsync(tenantId, productId, track, ct)
           ?? throw new KeyNotFoundException("Product not found.");

    private static string ValidateName(string? raw)
    {
        var name = raw?.Trim() ?? string.Empty;
        if (name.Length == 0) throw new ArgumentException("Variant name is required.");
        if (name.Length > 100) throw new ArgumentException("Variant name is too long (max 100).");
        return name;
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0) throw new ArgumentException("Price cannot be negative.");
    }

    private static void EnsureNameFree(Product product, string name, Guid? exceptId)
    {
        var clash = product.Variants.Any(v =>
            v.Id != exceptId && string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

        if (clash)
            throw new InvalidOperationException($"A variant named '{name}' already exists for this product.");
    }
}