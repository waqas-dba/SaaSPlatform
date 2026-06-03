// CoreKit.Catalog/Interfaces/IProductService.cs
using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken ct = default);

    Task<ProductDto> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken ct = default);

    Task<ProductDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<List<ProductDto>> GetByStoreAsync(
        Guid storeId,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken ct = default);
}