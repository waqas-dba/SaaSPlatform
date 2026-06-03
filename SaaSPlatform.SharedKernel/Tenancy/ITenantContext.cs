namespace CoreKit.SharedKernel.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
}