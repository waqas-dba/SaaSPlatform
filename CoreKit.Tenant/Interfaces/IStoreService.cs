using CoreKit.Tenant.Models;

namespace CoreKit.Tenant.Interfaces;

public interface IStoreService
{
    Task<StoreDto?> GetByIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default);

    Task<List<StoreDto>> GetAllByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<List<StoreDto>> GetAllStoresAsync(
        CancellationToken cancellationToken = default);

    Task<StoreDto> CreateAsync(
        CreateStoreRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid storeId,
        UpdateStoreRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid storeId,
        CancellationToken cancellationToken = default);

    Task SetMarketplaceListingAsync(
        Guid storeId,
        bool isListed,
        CancellationToken cancellationToken = default);
}