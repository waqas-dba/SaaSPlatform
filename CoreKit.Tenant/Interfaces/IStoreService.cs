using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Models;

namespace CoreKit.Tenant.Interfaces;

public interface IStoreService
{
    Task<Store?> GetByIdAsync(Guid storeId);
    Task<List<Store>> GetAllByTenantAsync(Guid tenantId);
    Task<Store> CreateAsync(Guid tenantId, string name, string? type = null, string? metadataJson = null);
    Task UpdateAsync(Guid storeId, UpdateStoreRequest request);
    Task DeleteAsync(Guid storeId);
    Task SetMarketplaceListingAsync(Guid storeId, bool isListed);
}