using CoreKit.Contracts.Models;

namespace CoreKit.Contracts.Interfaces;

public interface IStoreService
{
    Task<StoreDto?> GetByIdAsync(Guid storeId);
    Task<List<StoreDto>> GetAllByTenantAsync(Guid tenantId);
    Task<StoreDto> CreateAsync(Guid tenantId, string name, string? type = null, string? metadataJson = null);
    Task UpdateAsync(Guid storeId, UpdateStoreRequest request);
    Task DeleteAsync(Guid storeId);
    Task SetMarketplaceListingAsync(Guid storeId, bool isListed);
}