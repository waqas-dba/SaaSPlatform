using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface ITenantAccountRepository
{
    Task<TenantAccount?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    void Add(TenantAccount tenant);
    void Update(TenantAccount tenant);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);   // NEW
}