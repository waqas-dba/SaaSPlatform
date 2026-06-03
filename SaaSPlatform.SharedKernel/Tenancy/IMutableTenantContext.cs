namespace CoreKit.SharedKernel.Tenancy;

public interface IMutableTenantContext : ITenantContext
{
    void SetTenant(Guid? tenantId);
}