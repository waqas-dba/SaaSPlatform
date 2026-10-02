using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetTreeAsync(Guid tenantId, CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(Guid tenantId, CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid tenantId, Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}