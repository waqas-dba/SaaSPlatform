using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductVariantService
{
    Task<List<ProductVariantDto>> GetByProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
    Task<ProductVariantDto?> GetByIdAsync(Guid tenantId, Guid productId, Guid variantId, CancellationToken ct = default);
    Task<ProductVariantDto> CreateAsync(Guid tenantId, Guid productId, CreateVariantRequest request, CancellationToken ct = default);
    Task<ProductVariantDto> UpdateAsync(Guid tenantId, Guid productId, Guid variantId, UpdateVariantRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid productId, Guid variantId, CancellationToken ct = default);
}