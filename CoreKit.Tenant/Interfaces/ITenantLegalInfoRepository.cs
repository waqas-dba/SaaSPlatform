using CoreKit.Tenant.Entities;

namespace CoreKit.Tenant.Interfaces;

public interface ITenantLegalInfoRepository
{
    Task<TenantLegalInfo?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddOrUpdateAsync(TenantLegalInfo info, CancellationToken ct = default);
}