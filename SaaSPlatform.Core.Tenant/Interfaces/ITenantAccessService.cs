namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface ITenantAccessService
{
    Task<bool> TenantExistsAsync(Guid tenantId);
}