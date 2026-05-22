using CoreKit.Tenant.Entities;

namespace CoreKit.Tenant.Interfaces;

public interface IStoreTypeRepository
{
    Task<List<StoreType>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}