using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface ITenantLegalInfoRepository
{
    void Add(TenantLegalInfo info);
}