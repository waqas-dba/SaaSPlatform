using CoreKit.Tenant.Entities;

namespace CoreKit.Tenant.Interfaces;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(Guid storeId, CancellationToken ct = default);
    Task<List<Store>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
    void Add(Store store);
    void Update(Store store);
    void Delete(Store store);
}