using TenantKit.Entities;

namespace TenantKit.Interfaces;

public interface ITenantLegalInfoService
{
    Task AddOrUpdateAsync(Guid tenantId, string? businessLicenseNumber, string? taxId, string? additionalJson);
    Task<TenantLegalInfo?> GetAsync(Guid tenantId);
}