namespace CoreKit.SharedKernel.Common;

public abstract class BaseTenantEntity : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
}