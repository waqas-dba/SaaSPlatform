// FILE: CoreKit.Catalog/Interfaces/ICategoryService.cs  (updated)
// FIX: Added CancellationToken to every interface method so callers can
//      pass tokens and the explicit interface implementation bridges are
//      no longer needed. The old bridging pattern (explicit + async overload)
//      was fragile and easy to misuse.

using CoreKit.Catalog.Models;

namespace CoreKit.Catalog.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetByTenantAsync(
        Guid tenantId,
        Guid? storeId = null,
        CancellationToken ct = default);

    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken ct = default);

    Task UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}