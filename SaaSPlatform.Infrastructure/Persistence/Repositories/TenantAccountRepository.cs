using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class TenantAccountRepository : ITenantAccountRepository
{
    private readonly SaaSPlatformDbContext _db;

    public TenantAccountRepository(SaaSPlatformDbContext db) => _db = db;

    public async Task<TenantAccount?> GetByIdAsync(Guid tenantId, CancellationToken ct)
        => await _db.TenantAccounts.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId, ct);

    public void Add(TenantAccount tenant) => _db.TenantAccounts.Add(tenant);
    public void Update(TenantAccount tenant) => _db.TenantAccounts.Update(tenant);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.TenantAccounts.AnyAsync(t => t.Name == name, ct);
}