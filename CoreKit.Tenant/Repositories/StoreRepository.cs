using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly TenantDbContext _db;

    public StoreRepository(TenantDbContext db) => _db = db;

    public async Task<Store?> GetByIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Stores.FirstOrDefaultAsync(s => s.Id == storeId, ct);

    public async Task<Store?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Stores.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

    public async Task<List<Store>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Stores.Where(s => s.TenantId == tenantId).ToListAsync(ct);

    public void Add(Store store) => _db.Stores.Add(store);
    public void Update(Store store) => _db.Stores.Update(store);
    public void Delete(Store store) => _db.Stores.Remove(store);
}