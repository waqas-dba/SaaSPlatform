using CoreKit.Tenant.Models;

namespace CoreKit.Tenant.Interfaces;

public interface IStoreTypeService
{
    Task<List<StoreTypeDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}