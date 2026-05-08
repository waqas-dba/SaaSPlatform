using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly SaaSPlatformDbContext _db;
    public StoreRepository(SaaSPlatformDbContext db) => _db = db;
    public void Add(Store store) => _db.Stores.Add(store);
}