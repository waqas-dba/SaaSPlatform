using Microsoft.EntityFrameworkCore;
using TenantKit.Entities;
using TenantKit.Interfaces;

namespace TenantKit.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ITenantKitDbContext _db;
    public TenantRepository(ITenantKitDbContext db) => _db = db;

    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);

    public async Task<List<Tenant>> GetAllAsync(CancellationToken ct = default)
        => await _db.Tenants.ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.Tenants.AnyAsync(t => t.Name == name, ct);

    public void Add(Tenant tenant) => _db.Tenants.Add(tenant);
    public void Update(Tenant tenant) => _db.Tenants.Update(tenant);
}