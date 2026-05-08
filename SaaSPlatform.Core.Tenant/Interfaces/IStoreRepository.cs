using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface IStoreRepository
{
    void Add(Store store);
}