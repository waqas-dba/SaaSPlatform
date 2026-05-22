using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Repositories;

public class StoreTypeRepository : IStoreTypeRepository
{
    private readonly TenantDbContext _db;

    public StoreTypeRepository(TenantDbContext db) => _db = db;

    public async Task<List<StoreType>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _db.StoreTypes
            .AsNoTracking()
            .Where(st => st.IsActive)
            .OrderBy(st => st.SortOrder)
            .ToListAsync(cancellationToken);
    }
}