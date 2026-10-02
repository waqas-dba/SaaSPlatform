using CoreKit.Catalog.Models;
using CoreKit.SharedKernel.Models;

namespace CoreKit.Catalog.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateAsync(Guid tenantId, CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid tenantId, Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PagedResult<ProductListDto>> SearchAsync(Guid tenantId, ProductFilterQuery filter, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}