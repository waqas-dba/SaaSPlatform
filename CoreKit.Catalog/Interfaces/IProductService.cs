// CoreKit.Catalog/Interfaces/IProductService.cs
using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;

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

    Task<IReadOnlyList<ProductDto>> GetByStoreAsync(
        Guid storeId,
        CancellationToken ct = default);

    Task<PagedResult<ProductDto>> GetByStorePagedAsync(
        Guid storeId,
        PagedQuery query,
        CancellationToken ct = default);

    // NEW
    Task<PagedResult<ProductListDto>> SearchAsync(
        Guid storeId,
        ProductFilterQuery filter,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken ct = default);


}