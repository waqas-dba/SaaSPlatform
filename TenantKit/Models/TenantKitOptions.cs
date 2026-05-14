namespace TenantKit.Models;

public class TenantKitOptions
{
    /// <summary>If true, new tenants are activated immediately.</summary>
    public bool AutoApproveTenants { get; set; } = false;

    /// <summary>If true, a tenant can create more than one store/branch (subject to plan limits if IStoreLimitService is registered).</summary>
    public bool AllowMultipleStores { get; set; } = false;

    /// <summary>If true, the TenantLegalInfo service and repository are registered.</summary>
    public bool EnableLegalInfo { get; set; } = false;
}