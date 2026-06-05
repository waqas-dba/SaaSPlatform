// CoreKit.Catalog/Interfaces/IProductImageService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductImageService
{
    Task<ProductImageDto> AddAsync(
        Guid productId, AddProductImageRequest request,
        CancellationToken ct = default);

    Task RemoveAsync(Guid imageId, CancellationToken ct = default);

    Task ReorderAsync(
        Guid productId, ReorderImagesRequest request,
        CancellationToken ct = default);

    Task SetPrimaryAsync(Guid imageId, CancellationToken ct = default);
}