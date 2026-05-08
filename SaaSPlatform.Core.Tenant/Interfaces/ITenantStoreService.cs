// SaaSPlatform.Core/Tenant/Interfaces/ITenantStoreService.cs
using SaaSPlatform.Core.Tenant.Models;

namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface ITenantStoreService
{
    Task UpdateStoreAsync(Guid tenantId, UpdateStoreRequest request);
}