// FILE: CoreKit.Catalog/Interfaces/IProductService.cs  (updated)
// FIX: Added paginated overload. Non-paged version retained for compatibility.

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

    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns ALL products — prefer the paged overload for API endpoints.</summary>
    Task<IReadOnlyList<ProductDto>> GetByStoreAsync(
        Guid storeId,
        CancellationToken ct = default);

    /// <summary>Returns a paged slice of products for a store.</summary>
    Task<PagedResult<ProductDto>> GetByStorePagedAsync(
        Guid storeId,
        PagedQuery query,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

