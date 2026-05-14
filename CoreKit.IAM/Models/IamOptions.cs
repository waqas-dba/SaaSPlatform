using Microsoft.AspNetCore.Http;

namespace CoreKit.IAM.Models;

public class IamOptions
{
    /// <summary>Defines which field(s) are accepted for login ("phone", "email", "both").</summary>
    public string LoginIdentifier { get; set; } = "phone";

    /// <summary>Delegate to extract tenant ID from HttpContext.</summary>
    public Func<HttpContext, Guid?>? TenantResolver { get; set; }

    /// <summary>SuperAdmin role name (default "SuperAdmin").</summary>
    public string SuperAdminRoleName { get; set; } = "SuperAdmin";

    /// <summary>If true, users with the SuperAdmin role bypass all permission checks.</summary>
    public bool SuperAdminBypassPermissions { get; set; } = true;

    /// <summary>Minimum password length (used by validation).</summary>
    public int MinPasswordLength { get; set; } = 8;

    // ---------- Document‑related toggles ----------

    /// <summary>If true, the UserDocument service is registered.</summary>
    public bool EnableUserDocuments { get; set; } = false;

    /// <summary>If true, the UserIdentity service is registered (requires EnableUserDocuments = true).</summary>
    public bool EnableUserIdentities { get; set; } = false;

    /// <summary>When true, a CNIC number is required at registration time. Only effective if EnableUserIdentities is also true.</summary>
    public bool RequireCnic { get; set; } = true;

    /// <summary>If true, the RoleDocumentRequirement service is registered (requires EnableUserDocuments = true).</summary>
    public bool EnableRoleDocumentRequirements { get; set; } = false;
}