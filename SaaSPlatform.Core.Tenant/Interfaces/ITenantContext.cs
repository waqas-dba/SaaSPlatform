namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; set; }
}