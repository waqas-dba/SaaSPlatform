namespace SaaSPlatform.Core.Tenant.Constants;

/// <summary>
/// Centralized tenant-related constants.
/// Avoids magic strings across the application.
/// </summary>
public static class TenantConstants
{
    /// <summary>
    /// Request header used for tenant identification.
    /// </summary>
    public const string TenantHeader = "X-Tenant-ID";

    /// <summary>
    /// HttpContext.Items key used for storing current tenant.
    /// </summary>
    public const string TenantContextKey = "TenantId";
}