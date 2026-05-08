
namespace SaaSPlatform.SharedKernel.Common;

/// <summary>
/// Base entity for tenant scoped entities.
/// </summary>
public abstract class BaseTenantEntity
    : AuditableEntity,
      ITenantScoped
{
    /// <summary>
    /// Tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }
}