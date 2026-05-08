// Infrastructure/Persistence/Repositories/TenantRepository.cs
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly SaaSPlatformDbContext _db;

    public TenantRepository(SaaSPlatformDbContext db) => _db = db;

    public async Task<TenantAccount?> GetByIdAsync(Guid tenantId, CancellationToken ct)
        => await _db.TenantAccounts.IgnoreQueryFilters()       // Admin must see all tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

    public void Update(TenantAccount tenant) => _db.TenantAccounts.Update(tenant);
}