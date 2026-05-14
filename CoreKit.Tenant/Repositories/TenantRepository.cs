using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly TenantDbContext _db;

    public TenantRepository(TenantDbContext db) => _db = db;

    public async Task<TenantEntity?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);

    public async Task<List<TenantEntity>> GetAllAsync(CancellationToken ct = default)
        => await _db.Tenants.ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.Tenants.AnyAsync(t => t.Name == name, ct);

    public void Add(TenantEntity tenant) => _db.Tenants.Add(tenant);
    public void Update(TenantEntity tenant) => _db.Tenants.Update(tenant);
}