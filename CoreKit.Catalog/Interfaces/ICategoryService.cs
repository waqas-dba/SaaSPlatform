using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetByTenantAsync(
        Guid tenantId, Guid? storeId = null);

    Task<CategoryDto?> GetByIdAsync(Guid id);

    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);

    Task UpdateAsync(Guid id, UpdateCategoryRequest request);

    Task DeleteAsync(Guid id);
}