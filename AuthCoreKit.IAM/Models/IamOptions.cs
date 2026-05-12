using Microsoft.AspNetCore.Http;                     // <-- ADD THIS

namespace AuthCoreKit.IAM.Models;

public class IamOptions
{
    /// <summary>
    /// Defines which field(s) are accepted for login.
    /// Accepted values: "phone", "email", "both".
    /// Default is "phone".
    /// </summary>
    public string LoginIdentifier { get; set; } = "phone";

    /// <summary>If not null, this delegate extracts the tenant ID from the HttpContext.</summary>
    public Func<HttpContext, Guid?>? TenantResolver { get; set; }

    /// <summary>SuperAdmin role name (default "SuperAdmin").</summary>
    public string SuperAdminRoleName { get; set; } = "SuperAdmin";

    /// <summary>If true, users with the SuperAdmin role bypass all permission checks.</summary>
    public bool SuperAdminBypassPermissions { get; set; } = true;

    /// <summary>Minimum password length (used by validation).</summary>
    public int MinPasswordLength { get; set; } = 8;
}