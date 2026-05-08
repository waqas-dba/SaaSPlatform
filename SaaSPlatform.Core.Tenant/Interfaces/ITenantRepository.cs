// SaaSPlatform.Core/Tenant/Interfaces/ITenantRepository.cs

using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface ITenantRepository
{
    Task<TenantAccount?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    void Update(TenantAccount tenant);
    // Add any other needed methods
}