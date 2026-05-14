using TenantKit.Entities;

namespace TenantKit.Interfaces;

public interface ITenantLegalInfoRepository
{
    Task<TenantLegalInfo?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddOrUpdateAsync(TenantLegalInfo info, CancellationToken ct = default);
}