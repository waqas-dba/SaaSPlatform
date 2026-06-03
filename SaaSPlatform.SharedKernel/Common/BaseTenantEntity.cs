namespace CoreKit.SharedKernel.Common;

public abstract class BaseTenantEntity : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public void EnsureTenantAssigned()
    {
        if (TenantId == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} requires a valid TenantId.");
        }
    }
}