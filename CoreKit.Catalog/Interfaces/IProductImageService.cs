using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductImageService
{
    Task<ProductImageDto> AddAsync(Guid tenantId, Guid productId, AddProductImageRequest request, CancellationToken ct = default);
    Task RemoveAsync(Guid tenantId, Guid productId, Guid imageId, CancellationToken ct = default);
    Task ReorderAsync(Guid tenantId, Guid productId, ReorderImagesRequest request, CancellationToken ct = default);
    Task SetPrimaryAsync(Guid tenantId, Guid productId, Guid imageId, CancellationToken ct = default);
}