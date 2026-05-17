using CoreKit.Tenant.Models;

namespace CoreKit.Tenant.Interfaces;

/// <summary>
/// Store management service.
/// </summary>
public interface IStoreService
{
    Task<StoreDto?> GetByIdAsync(Guid storeId);
    Task<List<StoreDto>> GetAllByTenantAsync(Guid tenantId);
    Task<List<StoreDto>> GetAllStoresAsync();   // ← new: SuperAdmin global list
    Task<StoreDto> CreateAsync(CreateStoreRequest request);
    Task UpdateAsync(Guid storeId, UpdateStoreRequest request);
    Task DeleteAsync(Guid storeId);
    Task SetMarketplaceListingAsync(Guid storeId, bool isListed);
}